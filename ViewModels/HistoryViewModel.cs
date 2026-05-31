using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BayBrain.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private List<RepairOrder> _allOrders = new();

        // ── Search / filter ──────────────────────────────────────────────
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private string _filterCategory = "All";
        [ObservableProperty] private string _sortBy = "Newest";
        [ObservableProperty] private ObservableCollection<string> _categoryFilters = new(new[]
        {
            "All","Oil & Fluids","Brakes","Tires & Alignment","Filters",
            "Battery & Electrical","Suspension & Steering","Scheduled Maintenance",
            "Heating & Cooling","Safety & Visibility"
        });
        [ObservableProperty] private ObservableCollection<string> _sortOptions = new(new[]
        {
            "Newest","Oldest","Customer A→Z","Most Services","Highest Approval"
        });

        // ── Results ──────────────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<ROHistoryRow> _filteredOrders = new();
        [ObservableProperty] private int _totalCount = 0;
        [ObservableProperty] private bool _hasResults = false;
        [ObservableProperty] private bool _isEmpty = true;

        // ── Detail panel ─────────────────────────────────────────────────
        [ObservableProperty] private ROHistoryRow? _selectedOrder = null;
        [ObservableProperty] private bool _hasSelectedOrder = false;
        [ObservableProperty] private ObservableCollection<PerformedServiceRow> _selectedServices = new();

        // ── Summary stats ────────────────────────────────────────────────
        [ObservableProperty] private int _uniqueVehicles = 0;
        [ObservableProperty] private int _uniqueCustomers = 0;
        [ObservableProperty] private double _avgApprovalRate = 0;
        [ObservableProperty] private string _topService = "—";

        partial void OnSearchQueryChanged(string value) => ApplyFilter();
        partial void OnFilterCategoryChanged(string value) => ApplyFilter();
        partial void OnSortByChanged(string value) => ApplyFilter();

        public void Load(List<RepairOrder> orders)
        {
            _allOrders = orders;
            ComputeSummary();
            ApplyFilter();
        }

        private void ComputeSummary()
        {
            UniqueVehicles  = _allOrders.Select(o => o.Vin).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().Count();
            UniqueCustomers = _allOrders.Select(o => o.CustomerName).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Count();
            AvgApprovalRate = _allOrders.Count > 0 ? _allOrders.Average(o => o.ApprovalRate) : 0;
            TopService = _allOrders
                .SelectMany(o => o.PerformedServices)
                .GroupBy(s => s.ServiceName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "—";
        }

        private void ApplyFilter()
        {
            var q = SearchQuery.ToLowerInvariant();
            var filtered = _allOrders.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(q))
                filtered = filtered.Where(o =>
                    o.CustomerName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    o.Make.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    o.Model.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    o.Vin.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    o.AdvisorName.Contains(q, StringComparison.OrdinalIgnoreCase));

            if (FilterCategory != "All")
                filtered = filtered.Where(o =>
                    o.PerformedServices.Any(s => s.Category == FilterCategory) ||
                    o.RecommendedServices.Any(s => s.Category == FilterCategory));

            filtered = SortBy switch
            {
                "Oldest"          => filtered.OrderBy(o => o.CreatedAt),
                "Customer A→Z"    => filtered.OrderBy(o => o.CustomerName),
                "Most Services"   => filtered.OrderByDescending(o => o.RecommendedCount),
                "Highest Approval"=> filtered.OrderByDescending(o => o.ApprovalRate),
                _                 => filtered.OrderByDescending(o => o.CreatedAt)
            };

            var rows = filtered.Select(o => new ROHistoryRow(o)).ToList();
            FilteredOrders = new ObservableCollection<ROHistoryRow>(rows);
            TotalCount = rows.Count;
            HasResults = rows.Count > 0;
            IsEmpty = rows.Count == 0;
        }

        [RelayCommand]
        private void SelectOrder(ROHistoryRow? row)
        {
            SelectedOrder    = row;
            HasSelectedOrder = row != null;
            if (row == null) return;

            var services = row.Order.PerformedServices
                .Select(s => new PerformedServiceRow
                {
                    Name     = s.ServiceName,
                    Category = s.Category,
                    Price    = s.ActualPrice > 0 ? $"${s.ActualPrice:F2}" : "—",
                    Icon     = CategoryIcon(s.Category),
                    IsApproved = true
                })
                .Concat(row.Order.RecommendedServices
                    .Where(s => !s.IsApproved)
                    .Select(s => new PerformedServiceRow
                    {
                        Name     = s.ServiceName,
                        Category = s.Category,
                        Price    = "—",
                        Icon     = CategoryIcon(s.Category),
                        IsApproved = false
                    }))
                .ToList();

            SelectedServices = new ObservableCollection<PerformedServiceRow>(services);
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedOrder    = null;
            HasSelectedOrder = false;
            SelectedServices.Clear();
        }

        [RelayCommand]
        private void SetCategory(string? cat)
        {
            FilterCategory = cat ?? "All";
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery     = string.Empty;
            FilterCategory  = "All";
            SortBy          = "Newest";
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

    public class ROHistoryRow
    {
        public RepairOrder Order { get; }
        public string CustomerName  => Order.CustomerName;
        public string VehicleLabel  => string.IsNullOrWhiteSpace(Order.VehicleLabel.Trim()) ? "Unknown Vehicle" : Order.VehicleLabel;
        public string AdvisorName   => Order.AdvisorName;
        public string DateLabel     => Order.CreatedAt.ToString("MMM d, yyyy  h:mm tt");
        public string ApprovalLabel => $"{Order.ApprovedCount}/{Order.RecommendedCount} approved";
        public string ApprovalRate  => $"{Order.ApprovalRate:F0}%";
        public string ApprovalColor => Order.ApprovalRate switch
        {
            >= 80 => "#30D158",
            >= 50 => "#FFD60A",
            _     => "#FF453A"
        };
        public string StatusLabel   => Order.IsClosed ? "Closed" : "Open";
        public string StatusColor   => Order.IsClosed ? "#636366" : "#0A84FF";
        public bool HasCustomer     => !string.IsNullOrWhiteSpace(Order.CustomerName);
        public bool HasAdvisor      => !string.IsNullOrWhiteSpace(Order.AdvisorName);
        public string MileageLabel  => Order.Mileage > 0 ? $"{Order.Mileage:N0} mi" : "—";
        public string PhoneLabel    => Order.CustomerPhone;
        public bool HasPhone        => !string.IsNullOrWhiteSpace(Order.CustomerPhone);

        public ROHistoryRow(RepairOrder order) => Order = order;
    }

    public class PerformedServiceRow
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Price { get; set; } = "—";
        public string Icon { get; set; } = "🔧";
        public bool IsApproved { get; set; }
        public string StatusLabel => IsApproved ? "Performed" : "Declined";
        public string StatusColor => IsApproved ? "#30D158" : "#636366";
    }
}
