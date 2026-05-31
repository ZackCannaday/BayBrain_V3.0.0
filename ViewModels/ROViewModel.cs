using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BayBrain.ViewModels
{
    public partial class ROViewModel : ObservableObject
    {
        private readonly RORecommendationEngine _engine;
        private readonly ScriptGeneratorService _scriptGen = new();
        private readonly UrgencyService _urgency = new();
        private List<RepairOrder> _allOrders = new();

        // ── Vehicle input ────────────────────────────────────────────────
        [ObservableProperty] private int _vehicleYear = DateTime.Now.Year;
        [ObservableProperty] private string _vehicleMake = string.Empty;
        [ObservableProperty] private string _vehicleModel = string.Empty;
        [ObservableProperty] private int _vehicleMileage = 0;
        [ObservableProperty] private string _vehicleVin = string.Empty;
        [ObservableProperty] private string _customerName = string.Empty;
        [ObservableProperty] private string _customerPhone = string.Empty;
        [ObservableProperty] private string _advisorId = string.Empty;
        [ObservableProperty] private string _advisorName = string.Empty;
        [ObservableProperty] private int _lastOilChangeMileage = 0;
        [ObservableProperty] private bool _hasLastOilMileage = false;

        // ── RO state ─────────────────────────────────────────────────────
        [ObservableProperty] private bool _isAnalyzed = false;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private ObservableCollection<RecommendedService> _recommendations = new();
        [ObservableProperty] private RecommendedService? _selectedRec;

        // ── Summary counts ───────────────────────────────────────────────
        [ObservableProperty] private int _criticalCount = 0;
        [ObservableProperty] private int _highCount = 0;
        [ObservableProperty] private int _mediumCount = 0;
        [ObservableProperty] private int _totalRecommended = 0;
        [ObservableProperty] private int _approvedCount = 0;

        // ── Script for selected service ──────────────────────────────────
        [ObservableProperty] private string _selectedScript = string.Empty;

        // ── History ──────────────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<RepairOrder> _recentOrders = new();
        [ObservableProperty] private bool _hasRecentOrders = false;

        // ── Year range for picker ────────────────────────────────────────
        public ObservableCollection<int> YearOptions { get; } = new(
            Enumerable.Range(DateTime.Now.Year - 30, 32).Reverse());

        public ROViewModel(SearchService search, UrgencyService urgency)
        {
            _engine = new RORecommendationEngine(search, urgency);
            _urgency = urgency;
            LoadHistory();
        }

        // ── Commands ─────────────────────────────────────────────────────

        [RelayCommand]
        private void Analyze()
        {
            if (VehicleMileage <= 0) return;

            IsLoading = true;
            var recs = _engine.Analyze(
                VehicleMileage,
                HasLastOilMileage ? LastOilChangeMileage : null);

            Recommendations = new ObservableCollection<RecommendedService>(recs);
            IsAnalyzed = true;
            IsLoading = false;

            var (crit, high, med, _) = _engine.GetTierCounts(recs);
            CriticalCount = crit;
            HighCount = high;
            MediumCount = med;
            TotalRecommended = recs.Count;
            ApprovedCount = 0;
            SelectedRec = null;
            SelectedScript = string.Empty;
        }

        [RelayCommand]
        private void Reset()
        {
            VehicleYear  = DateTime.Now.Year;
            VehicleMake  = string.Empty;
            VehicleModel = string.Empty;
            VehicleMileage = 0;
            VehicleVin   = string.Empty;
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            HasLastOilMileage = false;
            LastOilChangeMileage = 0;
            IsAnalyzed = false;
            Recommendations.Clear();
            SelectedScript = string.Empty;
            CriticalCount = HighCount = MediumCount = TotalRecommended = ApprovedCount = 0;
        }

        [RelayCommand]
        private void ToggleApproval(RecommendedService? rec)
        {
            if (rec == null) return;
            if (rec.IsApproved)
            {
                rec.IsApproved = false;
                ApprovedCount--;
            }
            else
            {
                rec.IsApproved = true;
                rec.IsDeclined = false;
                ApprovedCount++;
            }
            // Notify collection refresh
            var idx = Recommendations.IndexOf(rec);
            if (idx >= 0) { Recommendations.RemoveAt(idx); Recommendations.Insert(idx, rec); }
            OnPropertyChanged(nameof(ApprovedCount));
        }

        [RelayCommand]
        private void DeclineService(RecommendedService? rec)
        {
            if (rec == null) return;
            if (rec.IsApproved) ApprovedCount--;
            rec.IsApproved = false;
            rec.IsDeclined = true;
            var idx = Recommendations.IndexOf(rec);
            if (idx >= 0) { Recommendations.RemoveAt(idx); Recommendations.Insert(idx, rec); }
            OnPropertyChanged(nameof(ApprovedCount));
        }

        [RelayCommand]
        private void ApproveAll()
        {
            foreach (var rec in Recommendations)
            {
                rec.IsApproved = true;
                rec.IsDeclined = false;
            }
            ApprovedCount = Recommendations.Count;
            RefreshList();
        }

        [RelayCommand]
        private void SaveRO()
        {
            if (!IsAnalyzed || Recommendations.Count == 0) return;

            var ro = new RepairOrder
            {
                Year          = VehicleYear,
                Make          = VehicleMake,
                Model         = VehicleModel,
                Mileage       = VehicleMileage,
                Vin           = VehicleVin,
                CustomerName  = CustomerName,
                CustomerPhone = CustomerPhone,
                AdvisorId     = AdvisorId,
                AdvisorName   = AdvisorName,
                RecommendedServices = Recommendations.ToList(),
                PerformedServices = Recommendations
                    .Where(r => r.IsApproved)
                    .Select(r => new PerformedService
                    {
                        ServiceId   = r.ServiceId,
                        ServiceName = r.ServiceName,
                        Category    = r.Category,
                        MileageAtService = VehicleMileage
                    }).ToList(),
                ClosedAt = DateTime.Now
            };

            _allOrders.Insert(0, ro);
            DataLoaderService.SaveRepairOrders(_allOrders);
            LoadHistory();
            OnROSaved?.Invoke(ro);
            Reset();
        }

        partial void OnSelectedRecChanged(RecommendedService? value)
        {
            if (value == null) { SelectedScript = string.Empty; return; }
            // Generate a quick script for this specific service
            SelectedScript =
                $"Based on the mileage on {(string.IsNullOrWhiteSpace(CustomerName) ? "your vehicle" : CustomerName + "'s " + VehicleMake + " " + VehicleModel)}:\n\n" +
                value.Reason + "\n\n" +
                $"The {value.ServiceName} is {value.UrgencyLabel.ToLower()} urgency — " +
                $"priced at {value.PriceRange}. " +
                (value.UrgencyScore >= 7
                    ? "I'd strongly recommend we take care of this today while you're already here."
                    : "Would you like to schedule this during today's visit?");
        }

        private void LoadHistory()
        {
            _allOrders = DataLoaderService.LoadRepairOrders();
            RecentOrders = new ObservableCollection<RepairOrder>(
                _allOrders.OrderByDescending(o => o.CreatedAt).Take(10));
            HasRecentOrders = RecentOrders.Count > 0;
        }

        private void RefreshList()
        {
            var tmp = Recommendations.ToList();
            Recommendations.Clear();
            foreach (var r in tmp) Recommendations.Add(r);
        }

        public void SetAdvisor(AdvisorProfile? profile)
        {
            if (profile == null) return;
            AdvisorId   = profile.Id;
            AdvisorName = profile.Name;
        }

        /// <summary>
        /// Optional callback — invoked by SaveRO so MainViewModel can
        /// forward the new RO to History/Dashboard without tight coupling.
        /// </summary>
        public Action<RepairOrder>? OnROSaved { get; set; }
    }
}
