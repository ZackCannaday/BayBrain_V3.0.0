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
        [ObservableProperty] private bool _hasSelectedMake = false;
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

        public ObservableCollection<string> MakeOptions { get; } = new();
        public ObservableCollection<string> ModelOptions { get; } = new();

        private static readonly Dictionary<string, string[]> VehicleModelMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Acura"] = ["ILX", "Integra", "MDX", "RDX", "TLX"],
            ["Audi"] = ["A3", "A4", "A5", "A6", "Q3", "Q5", "Q7", "Q8"],
            ["BMW"] = ["2 Series", "3 Series", "4 Series", "5 Series", "X1", "X3", "X5", "X7"],
            ["Buick"] = ["Encore", "Encore GX", "Envision", "Enclave"],
            ["Cadillac"] = ["CT4", "CT5", "Escalade", "XT4", "XT5", "XT6"],
            ["Chevrolet"] = ["Blazer", "Colorado", "Corvette", "Equinox", "Malibu", "Silverado 1500", "Suburban", "Tahoe", "Traverse", "Trax"],
            ["Chrysler"] = ["300", "Pacifica", "Voyager"],
            ["Dodge"] = ["Challenger", "Charger", "Durango", "Grand Caravan"],
            ["Ford"] = ["Bronco", "Edge", "Escape", "Expedition", "Explorer", "F-150", "F-250", "Fusion", "Maverick", "Mustang", "Ranger"],
            ["GMC"] = ["Acadia", "Canyon", "Sierra 1500", "Sierra 2500HD", "Terrain", "Yukon"],
            ["Honda"] = ["Accord", "Civic", "CR-V", "Fit", "HR-V", "Odyssey", "Passport", "Pilot", "Ridgeline"],
            ["Hyundai"] = ["Elantra", "Kona", "Palisade", "Santa Fe", "Sonata", "Tucson", "Venue"],
            ["Infiniti"] = ["Q50", "Q60", "QX50", "QX60", "QX80"],
            ["Jeep"] = ["Cherokee", "Compass", "Gladiator", "Grand Cherokee", "Renegade", "Wrangler"],
            ["Kia"] = ["Carnival", "Forte", "K5", "Seltos", "Sorento", "Soul", "Sportage", "Telluride"],
            ["Lexus"] = ["ES", "GX", "IS", "LS", "NX", "RX", "TX", "UX"],
            ["Lincoln"] = ["Aviator", "Corsair", "Nautilus", "Navigator"],
            ["Mazda"] = ["CX-3", "CX-30", "CX-5", "CX-50", "CX-9", "Mazda3", "Mazda6", "MX-5 Miata"],
            ["Mercedes-Benz"] = ["A-Class", "C-Class", "E-Class", "GLA", "GLB", "GLC", "GLE", "S-Class"],
            ["Nissan"] = ["Altima", "Armada", "Frontier", "Kicks", "Maxima", "Murano", "Pathfinder", "Rogue", "Sentra", "Titan", "Versa"],
            ["Ram"] = ["1500", "2500", "3500", "ProMaster"],
            ["Subaru"] = ["Ascent", "Crosstrek", "Forester", "Impreza", "Legacy", "Outback", "WRX"],
            ["Tesla"] = ["Model 3", "Model S", "Model X", "Model Y"],
            ["Toyota"] = ["4Runner", "Camry", "Corolla", "Highlander", "Prius", "RAV4", "Sequoia", "Sienna", "Tacoma", "Tundra"],
            ["Volkswagen"] = ["Atlas", "Golf", "ID.4", "Jetta", "Passat", "Taos", "Tiguan"],
            ["Volvo"] = ["S60", "S90", "V60", "XC40", "XC60", "XC90"]
        };

        public ROViewModel(SearchService search, UrgencyService urgency)
        {
            _engine = new RORecommendationEngine(search, urgency);
            _urgency = urgency;
            foreach (var make in VehicleModelMap.Keys.OrderBy(m => m))
            {
                MakeOptions.Add(make);
            }
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
            HasSelectedMake = false;
            ModelOptions.Clear();
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

        partial void OnVehicleMakeChanged(string value)
        {
            HasSelectedMake = !string.IsNullOrWhiteSpace(value);
            var previousModel = VehicleModel;
            ModelOptions.Clear();

            if (VehicleModelMap.TryGetValue(value, out var models))
            {
                foreach (var model in models)
                {
                    ModelOptions.Add(model);
                }
            }

            if (!string.IsNullOrWhiteSpace(previousModel) &&
                !ModelOptions.Contains(previousModel, StringComparer.OrdinalIgnoreCase))
            {
                VehicleModel = string.Empty;
            }
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
