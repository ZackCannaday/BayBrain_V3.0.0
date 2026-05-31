using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BayBrain.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SearchService _search;
        private readonly UrgencyService _urgency = new();
        private readonly ScriptGeneratorService _scriptGen = new();
        private readonly ScriptPrintService _printService = new();

        // ── Navigation ──────────────────────────────────────────────────
        [ObservableProperty] private int _selectedTabIndex = 0;

        // ── Search ──────────────────────────────────────────────────────
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private ObservableCollection<ServiceItem> _searchResults = new();
        [ObservableProperty] private ServiceItem? _selectedService;
        [ObservableProperty] private bool _hasResults = false;
        [ObservableProperty] private bool _showEmptyState = false;

        // ── Service Detail ───────────────────────────────────────────────
        [ObservableProperty] private UrgencyScore? _currentUrgency;
        [ObservableProperty] private string _generatedScript = string.Empty;
        [ObservableProperty] private string _customerName = string.Empty;
        [ObservableProperty] private bool _hasSymptoms = false;
        [ObservableProperty] private int _milesOverdue = 0;

        // ── Categories ──────────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<string> _categories = new();
        [ObservableProperty] private string? _selectedCategory;

        // ── Sub-ViewModels ──────────────────────────────────────────────
        [ObservableProperty] private QuizViewModel _quiz;
        [ObservableProperty] private AdvisorProfileViewModel _advisorProfiles;
        [ObservableProperty] private ROViewModel _roMode;
        [ObservableProperty] private DashboardViewModel _dashboard;
        [ObservableProperty] private HistoryViewModel _history;
        [ObservableProperty] private SettingsViewModel _settings;

        // ── Status bar ──────────────────────────────────────────────────
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _hasStatus = false;

        // Loaded data kept for Dashboard refresh
        private List<RepairOrder> _allOrders = new();
        private AppSettings _appSettings = new();

        // ── Convenience aliases for XAML bindings ──────────────────────────
        public ROViewModel RO => RoMode;

        public MainViewModel()
        {
            var services = DataLoaderService.LoadServices();
            _search = new SearchService(services);

            _appSettings = DataLoaderService.LoadSettings();
            _allOrders   = DataLoaderService.LoadRepairOrders();

            // ── Profiles ──────────────────────────────────────────────
            _advisorProfiles = new AdvisorProfileViewModel();

            // ── Quiz ──────────────────────────────────────────────────
            _quiz = new QuizViewModel();
            _quiz.OnSessionCompleted = record => _advisorProfiles.RecordSession(record);

            // ── RO Mode ───────────────────────────────────────────────
            _roMode = new ROViewModel(_search, _urgency);
            _roMode.OnROSaved = order =>
            {
                _allOrders.Add(order);
                DataLoaderService.SaveRepairOrders(_allOrders);
                _history.Load(_allOrders);
                RefreshDashboard();
            };

            // ── History ───────────────────────────────────────────────
            _history = new HistoryViewModel();
            _history.Load(_allOrders);

            // ── Settings ──────────────────────────────────────────────
            _settings = new SettingsViewModel();
            _settings.Load(_appSettings);
            _settings.RefreshStats(
                _allOrders.Count,
                _advisorProfiles.Profiles.Count,
                services.Count,
                DataLoaderService.LoadQuiz().Count);

            // ── Dashboard ─────────────────────────────────────────────
            _dashboard = new DashboardViewModel();
            RefreshDashboard();

            // ── Populate categories ───────────────────────────────────
            foreach (var cat in _search.GetCategories())
                Categories.Add(cat);

            LoadAllServices();
        }

        private void RefreshDashboard()
        {
            _dashboard.Refresh(
                _advisorProfiles.Profiles.ToList(),
                _allOrders,
                _search.GetAllServices().Count,
                _appSettings);
        }

        private void LoadAllServices()
        {
            var all = _search.Search(string.Empty);
            SearchResults = new ObservableCollection<ServiceItem>(all);
            HasResults    = SearchResults.Count > 0;
            ShowEmptyState = false;
        }

        partial void OnSearchQueryChanged(string value)       => ExecuteSearch();
        partial void OnSelectedCategoryChanged(string? value) => ExecuteSearch();

        partial void OnSelectedServiceChanged(ServiceItem? value)
        {
            if (value == null) return;
            CurrentUrgency  = _urgency.Calculate(value, MilesOverdue, HasSymptoms);
            GeneratedScript = _scriptGen.Generate(value, CurrentUrgency, CustomerName, HasSymptoms);
        }

        partial void OnMilesOverdueChanged(int value)   => RefreshUrgencyAndScript();
        partial void OnHasSymptomsChanged(bool value)   => RefreshUrgencyAndScript();
        partial void OnCustomerNameChanged(string value) => RefreshUrgencyAndScript();

        // When the tab switches TO the Dashboard (index 5) refresh it
        partial void OnSelectedTabIndexChanged(int value)
        {
            if (value == 5) RefreshDashboard();
            if (value == 7)  // Settings tab
            {
                _settings.RefreshStats(
                    _allOrders.Count,
                    _advisorProfiles.Profiles.Count,
                    _search.GetAllServices().Count,
                    DataLoaderService.LoadQuiz().Count);
            }
        }

        private void RefreshUrgencyAndScript()
        {
            if (SelectedService == null) return;
            CurrentUrgency  = _urgency.Calculate(SelectedService, MilesOverdue, HasSymptoms);
            GeneratedScript = _scriptGen.Generate(SelectedService, CurrentUrgency, CustomerName, HasSymptoms);
        }

        [RelayCommand]
        private void ExecuteSearch()
        {
            var results = _search.Search(SearchQuery);
            if (!string.IsNullOrEmpty(SelectedCategory))
                results = results.Where(r => r.Category == SelectedCategory).ToList();

            SearchResults  = new ObservableCollection<ServiceItem>(results);
            HasResults     = SearchResults.Count > 0;
            ShowEmptyState = !HasResults && !string.IsNullOrWhiteSpace(SearchQuery);
        }

        [RelayCommand]
        private void SelectService(ServiceItem? item) => SelectedService = item;

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery      = string.Empty;
            SelectedCategory = null;
            SelectedService  = null;
            LoadAllServices();
        }

        // ── Print & Export ──────────────────────────────────────────────

        [RelayCommand]
        private void PrintScript()
        {
            if (SelectedService == null || CurrentUrgency == null || string.IsNullOrEmpty(GeneratedScript)) return;
            _printService.PrintScript(SelectedService, CurrentUrgency, GeneratedScript, CustomerName);
        }

        [RelayCommand]
        private void ExportScript()
        {
            if (SelectedService == null || CurrentUrgency == null || string.IsNullOrEmpty(GeneratedScript)) return;
            var html = _printService.ExportToHtml(SelectedService, CurrentUrgency, GeneratedScript, CustomerName);
            var name = $"BayBrain_{SelectedService.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}";
            bool saved = _printService.SaveHtmlToFile(html, name);
            if (saved) ShowStatus("Script exported and opened in browser.");
        }

        [RelayCommand]
        private void CopyScript()
        {
            if (!string.IsNullOrEmpty(GeneratedScript))
            {
                System.Windows.Clipboard.SetText(GeneratedScript);
                ShowStatus("Script copied to clipboard.");
            }
        }

        // ── Navigation ──────────────────────────────────────────────────

        [RelayCommand]
        private void StartQuiz()
        {
            Quiz.StartNewSession();
            SelectedTabIndex = 3; // Quiz is now tab 3
        }

        [RelayCommand]
        private void NavigateTo(string tabIndex)
        {
            if (int.TryParse(tabIndex, out int idx))
                SelectedTabIndex = idx;
        }

        // ── Status message (auto-clears after 4 s) ─────────────────────

        private System.Threading.CancellationTokenSource? _statusCts;

        private async void ShowStatus(string msg)
        {
            _statusCts?.Cancel();
            _statusCts    = new System.Threading.CancellationTokenSource();
            StatusMessage = msg;
            HasStatus     = true;
            try
            {
                await Task.Delay(4000, _statusCts.Token);
                HasStatus = false;
            }
            catch (TaskCanceledException) { /* replaced by newer message */ }
        }
    }
}
