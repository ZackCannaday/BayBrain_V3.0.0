using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BayBrain.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // ── Summary stats ────────────────────────────────────────────────
        [ObservableProperty] private int _totalServices = 0;
        [ObservableProperty] private int _totalROsToday = 0;
        [ObservableProperty] private int _totalROsAllTime = 0;
        [ObservableProperty] private int _totalAdvisors = 0;
        [ObservableProperty] private int _totalQuizSessions = 0;
        [ObservableProperty] private double _teamAvgAccuracy = 0;
        [ObservableProperty] private double _teamApprovalRate = 0;
        [ObservableProperty] private string _dealershipName = "Bay Auto Group";
        [ObservableProperty] private string _dealershipLocation = "Augusta, GA";

        // ── Top performers ───────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<AdvisorStatRow> _topAdvisors = new();
        [ObservableProperty] private bool _hasAdvisors = false;

        // ── Recent activity ──────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<ActivityItem> _recentActivity = new();
        [ObservableProperty] private bool _hasActivity = false;

        // ── Category breakdown ───────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<CategoryStatRow> _categoryStats = new();

        // ── RO trend (last 7 days) ───────────────────────────────────────
        [ObservableProperty] private ObservableCollection<DayBar> _weeklyROs = new();
        [ObservableProperty] private int _weeklyROMax = 1;

        // ── Urgent items today ───────────────────────────────────────────
        [ObservableProperty] private int _criticalOpenItems = 0;
        [ObservableProperty] private string _lastRefreshed = string.Empty;

        public void Refresh(List<AdvisorProfile> profiles,
                            List<RepairOrder> orders,
                            int catalogSize,
                            AppSettings settings)
        {
            DealershipName     = settings.DealershipName;
            DealershipLocation = settings.DealershipLocation;
            TotalServices      = catalogSize;
            TotalAdvisors      = profiles.Count;
            TotalROsAllTime    = orders.Count;
            TotalROsToday      = orders.Count(o => o.CreatedAt.Date == DateTime.Today);
            TotalQuizSessions  = profiles.Sum(p => p.TotalSessionsCompleted);
            TeamAvgAccuracy    = profiles.Count > 0
                ? profiles.Where(p => p.TotalSessionsCompleted > 0)
                          .Select(p => p.OverallAccuracy)
                          .DefaultIfEmpty(0)
                          .Average() : 0;
            TeamApprovalRate   = orders.Count > 0
                ? orders.Average(o => o.ApprovalRate) : 0;

            CriticalOpenItems  = orders
                .Where(o => !o.IsClosed)
                .Sum(o => o.RecommendedServices.Count(r => r.UrgencyScore >= 9 && !r.IsApproved));

            // Top advisors by accuracy
            var advisorRows = profiles
                .Where(p => p.TotalSessionsCompleted > 0)
                .OrderByDescending(p => p.OverallAccuracy)
                .Take(5)
                .Select((p, i) => new AdvisorStatRow
                {
                    Rank        = i + 1,
                    Name        = p.Name,
                    Initials    = p.Initials,
                    AvatarColor = p.AvatarColor,
                    Accuracy    = p.OverallAccuracy,
                    Sessions    = p.TotalSessionsCompleted,
                    RankTitle   = p.RankTitle,
                    AccuracyWidth = p.OverallAccuracy * 2.4  // max ~240px
                });
            TopAdvisors = new ObservableCollection<AdvisorStatRow>(advisorRows);
            HasAdvisors = TopAdvisors.Count > 0;

            // Recent activity feed (ROs + quiz sessions blended)
            var activity = new List<ActivityItem>();

            foreach (var ro in orders.OrderByDescending(o => o.CreatedAt).Take(8))
                activity.Add(new ActivityItem
                {
                    Icon = "🔧",
                    Title = $"RO: {ro.VehicleLabel}",
                    Detail = $"{ro.CustomerName}  ·  {ro.ApprovedCount}/{ro.RecommendedCount} approved",
                    Time = ro.CreatedAt,
                    TimeLabel = FormatRelative(ro.CreatedAt),
                    Color = "#0A84FF"
                });

            foreach (var p in profiles)
                foreach (var s in p.SessionHistory.OrderByDescending(x => x.CompletedAt).Take(3))
                    activity.Add(new ActivityItem
                    {
                        Icon = "🎯",
                        Title = $"Quiz: {p.Name}",
                        Detail = $"{s.CorrectCount}/{s.TotalQuestions}  ({s.ScorePercent:F0}%)" +
                                 (s.CategoryFilter != null ? $"  ·  {s.CategoryFilter}" : ""),
                        Time = s.CompletedAt,
                        TimeLabel = FormatRelative(s.CompletedAt),
                        Color = "#30D158"
                    });

            RecentActivity = new ObservableCollection<ActivityItem>(
                activity.OrderByDescending(a => a.Time).Take(12));
            HasActivity = RecentActivity.Count > 0;

            // Weekly RO bar chart
            var today = DateTime.Today;
            var weekBars = Enumerable.Range(0, 7).Select(i =>
            {
                var day = today.AddDays(-6 + i);
                var count = orders.Count(o => o.CreatedAt.Date == day);
                return new DayBar
                {
                    DayLabel = i == 6 ? "Today" : day.ToString("ddd"),
                    Count    = count,
                    IsToday  = i == 6
                };
            }).ToList();
            WeeklyROMax = Math.Max(1, weekBars.Max(b => b.Count));
            foreach (var bar in weekBars)
                bar.BarHeight = WeeklyROMax > 0 ? (int)(bar.Count * 80.0 / WeeklyROMax) : 0;
            WeeklyROs = new ObservableCollection<DayBar>(weekBars);

            // Category breakdown from RO performed services
            var catGroups = orders
                .SelectMany(o => o.PerformedServices)
                .GroupBy(s => s.Category)
                .Select(g => new CategoryStatRow
                {
                    Category = g.Key,
                    Count    = g.Count(),
                    Icon     = CategoryIcon(g.Key)
                })
                .OrderByDescending(c => c.Count)
                .ToList();
            int catMax = catGroups.Count > 0 ? catGroups.Max(c => c.Count) : 1;
            foreach (var c in catGroups) c.BarWidth = catMax > 0 ? (int)(c.Count * 200.0 / catMax) : 0;
            CategoryStats = new ObservableCollection<CategoryStatRow>(catGroups);

            LastRefreshed = $"Refreshed {DateTime.Now:h:mm tt}";
        }

        [RelayCommand]
        private void Refresh() { /* triggered by MainViewModel */ }

        private static string FormatRelative(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 2) return "just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return dt.ToString("MMM d");
        }

        private static string CategoryIcon(string cat) => cat switch
        {
            "Oil & Fluids"          => "🛢",
            "Brakes"                => "🛑",
            "Tires & Alignment"     => "⚙",
            "Filters"               => "🌬",
            "Battery & Electrical"  => "⚡",
            "Suspension & Steering" => "🔩",
            "Scheduled Maintenance" => "📅",
            "Heating & Cooling"     => "🌡",
            "Safety & Visibility"   => "👁",
            _                       => "🔧"
        };
    }

    public class AdvisorStatRow
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#0A84FF";
        public double Accuracy { get; set; }
        public int Sessions { get; set; }
        public string RankTitle { get; set; } = string.Empty;
        public double AccuracyWidth { get; set; }
        public string AccuracyLabel => $"{Accuracy:F0}%";
        public string RankDisplay => Rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{Rank}" };
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = "🔧";
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string TimeLabel { get; set; } = string.Empty;
        public string Color { get; set; } = "#8E8E93";
    }

    public class CategoryStatRow
    {
        public string Category { get; set; } = string.Empty;
        public string Icon { get; set; } = "🔧";
        public int Count { get; set; }
        public int BarWidth { get; set; }
    }

    public class DayBar
    {
        public string DayLabel { get; set; } = string.Empty;
        public int Count { get; set; }
        public int BarHeight { get; set; }
        public bool IsToday { get; set; }
        public string BarColor => IsToday ? "#0A84FF" : "#3A3A3C";
        public string LabelColor => IsToday ? "#0A84FF" : "#636366";
    }
}
