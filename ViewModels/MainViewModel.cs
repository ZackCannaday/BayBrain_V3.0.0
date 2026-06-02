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
        private readonly AuthService _auth;

        // ── Navigation ──────────────────────────────────────────────────
        [ObservableProperty] private int _selectedTabIndex = 0;

        // ── Search ──────────────────────────────────────────────────────
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private ObservableCollection<ServiceItem> _searchResults = new();
        [ObservableProperty] private ObservableCollection<ServiceCategoryGroup> _serviceGroups = new();
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

        // ── Local session / access control ───────────────────────────────
        [ObservableProperty] private ObservableCollection<UserAccount> _loginUsers = new();
        [ObservableProperty] private UserAccount? _selectedLoginUser;
        [ObservableProperty] private string _loginPin = string.Empty;
        [ObservableProperty] private string _loginMessage = "Select your user and enter your PIN.";
        [ObservableProperty] private UserAccount? _currentUser;

        public bool IsLoggedIn => CurrentUser != null;
        public bool CanAccessSettings => CurrentUser?.CanAccessSettings == true;
        public string CurrentUserDisplayName => CurrentUser?.DisplayName ?? "Not signed in";
        public string CurrentUserRoleLabel => CurrentUser?.RoleLabel ?? "Locked";

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
            _auth = new AuthService();
            RefreshLoginUsers();

            // ── Profiles ──────────────────────────────────────────────
            _advisorProfiles = new AdvisorProfileViewModel();
            _advisorProfiles.CanActivateProfile = profile =>
                CurrentUser?.CanAccessSettings == true || CurrentUser?.AdvisorProfileId == profile.Id;
            _advisorProfiles.ActivationDenied = ShowStatus;
            _advisorProfiles.ActiveAdvisorChanged = profile =>
            {
                RoMode.SetAdvisor(profile);
                ShowStatus(profile == null ? "No active advisor." : $"Active advisor: {profile.Name}");
            };

            // ── Quiz ──────────────────────────────────────────────────
            _quiz = new QuizViewModel();
            _quiz.OnSessionCompleted = record => _advisorProfiles.RecordSession(record);

            // ── History ───────────────────────────────────────────────
            _history = new HistoryViewModel();
            _history.Load(_allOrders);

            // ── RO Mode ───────────────────────────────────────────────
            _roMode = new ROViewModel(_search, _urgency);
            _roMode.OnROSaved = order =>
            {
                if (_allOrders.All(o => o.Id != order.Id))
                    _allOrders.Insert(0, order);
                History.Load(_allOrders);
                RefreshDashboard();
            };

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
            Dashboard.Refresh(
                AdvisorProfiles.Profiles.ToList(),
                _allOrders,
                _search.GetAllServices().Count,
                _appSettings);
        }

        private void LoadAllServices()
        {
            var all = _search.Search(string.Empty);
            SearchResults = new ObservableCollection<ServiceItem>(all);
            RefreshServiceGroups(all);
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

        partial void OnCurrentUserChanged(UserAccount? value)
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(CanAccessSettings));
            OnPropertyChanged(nameof(CurrentUserDisplayName));
            OnPropertyChanged(nameof(CurrentUserRoleLabel));
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
                Settings.RefreshStats(
                    _allOrders.Count,
                    AdvisorProfiles.Profiles.Count,
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
            RefreshServiceGroups(results);
            HasResults     = SearchResults.Count > 0;
            ShowEmptyState = !HasResults && !string.IsNullOrWhiteSpace(SearchQuery);
        }

        private void RefreshServiceGroups(IEnumerable<ServiceItem> services)
        {
            ServiceGroups = new ObservableCollection<ServiceCategoryGroup>(
                services
                    .GroupBy(s => s.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => new ServiceCategoryGroup(
                        g.Key,
                        new ObservableCollection<ServiceItem>(g.OrderBy(s => s.Name)))));
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
            {
                if (!IsLoggedIn)
                {
                    ShowStatus("Sign in before using BayBrain.");
                    return;
                }

                SelectedTabIndex = idx;
                if (idx == 7 && !CanAccessSettings)
                    ShowStatus("Settings are restricted to manager and admin users.");
            }
        }

        [RelayCommand]
        private void Login()
        {
            if (SelectedLoginUser == null)
            {
                LoginMessage = "Select a user first.";
                return;
            }

            if (!_auth.VerifyPin(SelectedLoginUser, LoginPin))
            {
                LoginPin = string.Empty;
                LoginMessage = "Incorrect PIN.";
                return;
            }

            CurrentUser = SelectedLoginUser;
            LoginPin = string.Empty;
            LoginMessage = $"Signed in as {CurrentUser.DisplayName}.";
            _auth.RecordLogin(CurrentUser);
            ApplyCurrentUserAdvisor();

            if (CanAccessSettings)
            {
                Settings.RefreshStats(
                    _allOrders.Count,
                    AdvisorProfiles.Profiles.Count,
                    _search.GetAllServices().Count,
                    DataLoaderService.LoadQuiz().Count);
            }
        }

        [RelayCommand]
        private void Logout()
        {
            CurrentUser = null;
            AdvisorProfiles.SetActiveAdvisorById(null);
            SelectedTabIndex = 0;
            RefreshLoginUsers();
            LoginMessage = "Select your user and enter your PIN.";
            ShowStatus("Signed out.");
        }

        private void RefreshLoginUsers()
        {
            _auth.Load();
            LoginUsers = new ObservableCollection<UserAccount>(_auth.Users.Where(u => u.IsActive).OrderBy(u => u.DisplayName));
            SelectedLoginUser = LoginUsers.FirstOrDefault(u => u.Role == UserRole.Admin) ?? LoginUsers.FirstOrDefault();
        }

        private void ApplyCurrentUserAdvisor()
        {
            if (CurrentUser == null)
            {
                AdvisorProfiles.SetActiveAdvisorById(null);
                return;
            }

            if (CurrentUser.Role == UserRole.Advisor)
            {
                AdvisorProfiles.SetActiveAdvisorById(CurrentUser.AdvisorProfileId);
                if (AdvisorProfiles.ActiveAdvisor == null)
                    ShowStatus("This advisor login is not linked to an advisor profile yet.");
            }
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

    public sealed class ServiceCategoryGroup
    {
        public string Category { get; }
        public ObservableCollection<ServiceItem> Services { get; }

        public ServiceCategoryGroup(string category, ObservableCollection<ServiceItem> services)
        {
            Category = category;
            Services = services;
        }
    }
}
