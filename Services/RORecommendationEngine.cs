using BayBrain.Models;
using System.Collections.Generic;

namespace BayBrain.Services
{
    /// <summary>
    /// Given a vehicle's mileage and known service history,
    /// determines which services are due, overdue, or upcoming.
    /// </summary>
    public class RORecommendationEngine
    {
        private readonly SearchService _search;
        private readonly UrgencyService _urgency;

        // Mileage-interval map: service ID → interval miles (0 = time-based only)
        private static readonly Dictionary<string, int> _mileageIntervals = new()
        {
            ["oil_change"]            = 5000,
            ["tire_rotation"]         = 6000,
            ["cabin_air_filter"]      = 20000,
            ["engine_air_filter"]     = 20000,
            ["fuel_filter"]           = 30000,
            ["transmission_fluid"]    = 45000,
            ["coolant_flush"]         = 30000,
            ["brake_fluid"]           = 30000,
            ["power_steering_fluid"]  = 40000,
            ["differential_fluid"]    = 40000,
            ["wheel_alignment"]       = 12000,
            ["tire_balance"]          = 12000,
            ["spark_plugs"]           = 30000,    // copper — iridium = 100k
            ["serpentine_belt"]       = 80000,
            ["timing_belt"]           = 80000,
            ["30k_service"]           = 30000,
            ["60k_service"]           = 60000,
            ["shocks_struts"]         = 70000,
            ["ball_joints"]           = 60000,
            ["cv_axle"]               = 80000,
            ["tie_rod"]               = 60000,
            ["wheel_bearing"]         = 90000,
            ["fuel_injection_cleaning"] = 35000,
        };

        // Mileage milestone triggers (service only triggered near a milestone)
        private static readonly Dictionary<string, int[]> _milestoneTriggers = new()
        {
            ["30k_service"] = new[] { 30000, 90000, 150000 },
            ["60k_service"] = new[] { 60000, 120000, 180000 },
        };

        public RORecommendationEngine(SearchService search, UrgencyService urgency)
        {
            _search = search;
            _urgency = urgency;
        }

        /// <summary>
        /// Core method: analyze vehicle mileage, return a prioritized
        /// list of recommended services with urgency scores and reasons.
        /// </summary>
        public List<RecommendedService> Analyze(int currentMileage,
                                                int? lastOilChangeMileage = null,
                                                HashSet<string>? recentlyPerformed = null)
        {
            recentlyPerformed ??= new HashSet<string>();
            var results = new List<(RecommendedService rec, int urgencyScore)>();

            foreach (var (serviceId, intervalMiles) in _mileageIntervals)
            {
                if (recentlyPerformed.Contains(serviceId)) continue;

                var service = _search.GetById(serviceId);
                if (service == null) continue;

                // Special case: milestone services
                if (_milestoneTriggers.TryGetValue(serviceId, out var milestones))
                {
                    var (hit, milesUntil) = CheckMilestone(currentMileage, milestones);
                    if (!hit && milesUntil > 5000) continue;   // not close enough

                    int milesOver = hit ? Math.Max(0, currentMileage - milestones
                        .Where(m => m <= currentMileage).DefaultIfEmpty(0).Max()) : 0;
                    var reason = hit
                        ? $"Vehicle is at or past the {FormatMiles(milestones.Where(m => m <= currentMileage).Max())} milestone"
                        : $"Upcoming {FormatMiles(milestones.First(m => m > currentMileage))} milestone in {FormatMiles(milesUntil)}";

                    var us = _urgency.Calculate(service, milesOver);
                    results.Add((BuildRec(service, us, reason), us.Score));
                    continue;
                }

                // Standard interval check
                // Assume last service was at the most recent interval boundary before current mileage
                // If we don't know when it was last done, we flag conservatively at >80% of interval
                int remainder = currentMileage % intervalMiles;
                int milesOverdue = remainder > (int)(intervalMiles * 0.85)
                    ? remainder - (int)(intervalMiles * 0.85) : 0;

                // Oil change: use explicit last-service mileage if provided
                if (serviceId == "oil_change" && lastOilChangeMileage.HasValue)
                {
                    int milesSinceOil = currentMileage - lastOilChangeMileage.Value;
                    if (milesSinceOil < (int)(intervalMiles * 0.85)) continue;
                    milesOverdue = Math.Max(0, milesSinceOil - intervalMiles);
                    var oilUs = _urgency.Calculate(service, milesOverdue);
                    var oilReason = milesOverdue > 0
                        ? $"Oil last changed {FormatMiles(milesSinceOil)} ago — {FormatMiles(milesOverdue)} overdue"
                        : $"Oil change due within {FormatMiles(intervalMiles - (currentMileage - lastOilChangeMileage.Value))}";
                    results.Add((BuildRec(service, oilUs, oilReason), oilUs.Score));
                    continue;
                }

                if (remainder < (int)(intervalMiles * 0.85)) continue; // not due yet

                var urgency = _urgency.Calculate(service, milesOverdue);
                string reason2 = milesOverdue > 0
                    ? $"Service interval of {FormatMiles(intervalMiles)} — approximately {FormatMiles(milesOverdue)} overdue"
                    : $"Approaching {FormatMiles(intervalMiles)} service interval";

                results.Add((BuildRec(service, urgency, reason2), urgency.Score));
            }

            // Sort: Critical → High → Medium, then by score descending
            return results
                .OrderByDescending(x => x.urgencyScore)
                .Select(x => x.rec)
                .ToList();
        }

        /// <summary>
        /// Quick summary: counts per urgency tier.
        /// </summary>
        public (int critical, int high, int medium, int low) GetTierCounts(
            List<RecommendedService> recs)
        {
            int crit = recs.Count(r => r.UrgencyScore >= 9);
            int high = recs.Count(r => r.UrgencyScore >= 7 && r.UrgencyScore < 9);
            int med  = recs.Count(r => r.UrgencyScore >= 4 && r.UrgencyScore < 7);
            int low  = recs.Count(r => r.UrgencyScore < 4);
            return (crit, high, med, low);
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static RecommendedService BuildRec(ServiceItem service,
                                                   UrgencyScore us, string reason) => new()
        {
            ServiceId       = service.Id,
            ServiceName     = service.Name,
            Category        = service.Category,
            UrgencyScore    = us.Score,
            UrgencyLabel    = us.Label,
            UrgencyColorHex = us.ColorHex,
            PriceRange      = service.PriceRange,
            Reason          = reason
        };

        private static (bool hit, int milesUntil) CheckMilestone(int mileage, int[] milestones)
        {
            foreach (var m in milestones.OrderBy(x => x))
            {
                int diff = m - mileage;
                if (diff <= 0 && diff > -5000) return (true, 0);   // at/just past
                if (diff > 0 && diff <= 4000)  return (false, diff); // approaching
            }
            return (false, int.MaxValue);
        }

        private static string FormatMiles(int miles) =>
            miles >= 1000 ? $"{miles / 1000}K miles" : $"{miles} miles";
    }
}
