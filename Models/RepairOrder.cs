using System.Collections.Generic;

namespace BayBrain.Models
{
    /// <summary>
    /// Represents one customer visit / repair order (RO).
    /// Stores vehicle context, mileage, recommended services, and what was performed.
    /// </summary>
    public class RepairOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // ── Vehicle ──────────────────────────────────────────────────────
        public int Year { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public string Vin { get; set; } = string.Empty;

        // ── Customer ─────────────────────────────────────────────────────
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // ── Advisor ──────────────────────────────────────────────────────
        public string AdvisorId { get; set; } = string.Empty;
        public string AdvisorName { get; set; } = string.Empty;

        // ── Services ─────────────────────────────────────────────────────
        /// <summary>Services the RO engine flagged as due/overdue.</summary>
        public List<RecommendedService> RecommendedServices { get; set; } = new();

        /// <summary>Services the customer approved and that were performed.</summary>
        public List<PerformedService> PerformedServices { get; set; } = new();

        // ── Timestamps ───────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }
        public bool IsClosed => ClosedAt.HasValue;

        // ── Computed ─────────────────────────────────────────────────────
        public string VehicleLabel => $"{Year} {Make} {Model}".Trim();
        public string MileageLabel => Mileage > 0 ? $"{Mileage:N0} mi" : "—";

        public decimal TotalPerformed =>
            PerformedServices.Sum(s => s.ActualPrice);

        public int ApprovedCount =>
            PerformedServices.Count;

        public int RecommendedCount =>
            RecommendedServices.Count;

        public double ApprovalRate => RecommendedCount > 0
            ? (double)ApprovedCount / RecommendedCount * 100 : 0;
    }

    public class RecommendedService
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int UrgencyScore { get; set; }
        public string UrgencyLabel { get; set; } = string.Empty;
        public string UrgencyColorHex { get; set; } = "#8E8E93";
        public string PriceRange { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;   // Why flagged
        public bool IsApproved { get; set; } = false;
        public bool IsDeclined { get; set; } = false;
        public string Status => IsApproved ? "Approved" : IsDeclined ? "Declined" : "Pending";
        public string StatusColor => IsApproved ? "#30D158" : IsDeclined ? "#FF453A" : "#8E8E93";
    }

    public class PerformedService
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal ActualPrice { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        public int MileageAtService { get; set; }
    }
}
