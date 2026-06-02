using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace BayBrain.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private AppSettings _settings = new();
        private readonly AuthService _auth = new();

        // ── Dealership info ──────────────────────────────────────────────
        [ObservableProperty] private string _dealershipName     = "Bay Auto Group";
        [ObservableProperty] private string _dealershipLocation = "Augusta, GA";
        [ObservableProperty] private string _dealershipPhone    = "(706) 555-0100";
        [ObservableProperty] private string _advisorSignature   = string.Empty;
        [ObservableProperty] private string _adminPin           = "0000";

        // ── Defaults ─────────────────────────────────────────────────────
        [ObservableProperty] private int _defaultUrgencyThreshold = 5;
        [ObservableProperty] private bool _autoSaveROsOnClose  = true;
        [ObservableProperty] private bool _playQuizSounds      = true;
        [ObservableProperty] private bool _showMileageWarnings = true;
        [ObservableProperty] private string _exportFolder      = string.Empty;

        // ── About ────────────────────────────────────────────────────────
        [ObservableProperty] private string _appVersion        = "3.0.0";
        [ObservableProperty] private string _buildDate         = "May 2026";

        // ── Status feedback ──────────────────────────────────────────────
        [ObservableProperty] private string _statusMessage     = string.Empty;
        [ObservableProperty] private string _statusColor       = "#30D158";
        [ObservableProperty] private bool   _showStatus        = false;

        // ── Data stats ───────────────────────────────────────────────────
        [ObservableProperty] private int _roCount       = 0;
        [ObservableProperty] private int _profileCount  = 0;
        [ObservableProperty] private int _serviceCount  = 0;
        [ObservableProperty] private int _quizCount     = 0;
        [ObservableProperty] private int _userCount     = 0;

        // ── User/account management ─────────────────────────────────────
        [ObservableProperty] private ObservableCollection<UserAccount> _users = new();
        [ObservableProperty] private ObservableCollection<AdvisorProfile> _advisorProfileOptions = new();
        [ObservableProperty] private ObservableCollection<string> _roleOptions = new(new[] { "Advisor", "Manager", "Admin" });
        [ObservableProperty] private string _newUsername = string.Empty;
        [ObservableProperty] private string _newDisplayName = string.Empty;
        [ObservableProperty] private string _newUserRole = "Advisor";
        [ObservableProperty] private string _newUserPin = string.Empty;
        [ObservableProperty] private AdvisorProfile? _selectedAdvisorForUser;

        public void Load(AppSettings settings)
        {
            _settings = settings;
            DealershipName          = settings.DealershipName;
            DealershipLocation      = settings.DealershipLocation;
            DealershipPhone         = settings.DealershipPhone;
            AdvisorSignature        = settings.DefaultAdvisorSignature;
            AdminPin                = string.IsNullOrWhiteSpace(settings.AdminPin) ? "0000" : settings.AdminPin;
            DefaultUrgencyThreshold = settings.DefaultUrgencyThreshold;
            AutoSaveROsOnClose      = settings.AutoSaveROsOnClose;
            PlayQuizSounds          = settings.PlayQuizSounds;
            ShowMileageWarnings     = settings.ShowMileageWarnings;
            ExportFolder            = settings.ExportFolder;
            RefreshUsers();
        }

        public AppSettings BuildSettings()
        {
            _settings.DealershipName          = DealershipName.Trim();
            _settings.DealershipLocation      = DealershipLocation.Trim();
            _settings.DealershipPhone         = DealershipPhone.Trim();
            _settings.DefaultAdvisorSignature  = AdvisorSignature.Trim();
            _settings.AdminPin                 = string.IsNullOrWhiteSpace(AdminPin) ? "0000" : AdminPin.Trim();
            _settings.DefaultUrgencyThreshold  = DefaultUrgencyThreshold;
            _settings.AutoSaveROsOnClose       = AutoSaveROsOnClose;
            _settings.PlayQuizSounds           = PlayQuizSounds;
            _settings.ShowMileageWarnings      = ShowMileageWarnings;
            _settings.ExportFolder             = ExportFolder.Trim();
            return _settings;
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var settings = BuildSettings();
                DataLoaderService.SaveSettings(settings);
                ShowFeedback("Settings saved successfully!", "#30D158");
            }
            catch (Exception ex)
            {
                ShowFeedback($"Error saving: {ex.Message}", "#FF453A");
            }
        }

        [RelayCommand]
        private void BrowseExportFolder()
        {
            // Use a SaveFileDialog as a folder picker workaround (WPF has no FolderBrowserDialog built in)
            var dlg = new OpenFileDialog
            {
                Title            = "Select Export Folder",
                CheckFileExists  = false,
                CheckPathExists  = true,
                FileName         = "Select Folder",
                Filter           = "All files (*.*)|*.*",
                ValidateNames    = false
            };
            if (dlg.ShowDialog() == true)
            {
                ExportFolder = Path.GetDirectoryName(dlg.FileName) ?? string.Empty;
            }
        }

        [RelayCommand]
        private void ExportAllData()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Title      = "Export All BayBrain Data",
                    Filter     = "ZIP Archive (*.zip)|*.zip",
                    FileName   = $"BayBrain_Export_{DateTime.Now:yyyyMMdd_HHmm}.zip"
                };
                if (dlg.ShowDialog() != true) return;

                var files = new[] { "services.json", "quiz.json", "advisor_profiles.json", "repair_orders.json", "settings.json", "users.json" };
                using var zip = System.IO.Compression.ZipFile.Open(dlg.FileName, System.IO.Compression.ZipArchiveMode.Create);
                foreach (var f in files)
                {
                    var path = FindDataFile(f);
                    if (File.Exists(path)) zip.CreateEntryFromFile(path, f);
                }
                ShowFeedback($"Exported to {dlg.FileName}", "#30D158");
            }
            catch (Exception ex)
            {
                ShowFeedback($"Export failed: {ex.Message}", "#FF453A");
            }
        }

        [RelayCommand]
        private void ImportData()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title  = "Import BayBrain Data (ZIP)",
                    Filter = "ZIP Archive (*.zip)|*.zip"
                };
                if (dlg.ShowDialog() != true) return;

                var dir = AppDomain.CurrentDomain.BaseDirectory;
                using var zip = System.IO.Compression.ZipFile.OpenRead(dlg.FileName);
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        var targetDir = entry.Name is "services.json" or "quiz.json"
                            ? Path.Combine(dir, "Data")
                            : dir;
                        Directory.CreateDirectory(targetDir);
                        entry.ExtractToFile(Path.Combine(targetDir, entry.Name), overwrite: true);
                    }
                }
                ShowFeedback("Data imported! Restart app to apply.", "#FFD60A");
            }
            catch (Exception ex)
            {
                ShowFeedback($"Import failed: {ex.Message}", "#FF453A");
            }
        }

        [RelayCommand]
        private void ClearROHistory()
        {
            if (!ConfirmDanger("Clear all repair order history? This cannot be undone.")) return;

            try
            {
                DataLoaderService.SaveRepairOrders(new List<RepairOrder>());
                RoCount = 0;
                ShowFeedback("RO history cleared.", "#FF453A");
            }
            catch (Exception ex)
            {
                ShowFeedback($"Error: {ex.Message}", "#FF453A");
            }
        }

        [RelayCommand]
        private void ClearProfiles()
        {
            if (!ConfirmDanger("Clear all advisor profiles and quiz history? This cannot be undone.")) return;

            try
            {
                DataLoaderService.SaveProfiles(new List<AdvisorProfile>());
                ProfileCount = 0;
                ShowFeedback("Advisor profiles cleared.", "#FF453A");
            }
            catch (Exception ex)
            {
                ShowFeedback($"Error: {ex.Message}", "#FF453A");
            }
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            Load(new AppSettings());
            ShowFeedback("Reset to defaults. Click Save to apply.", "#FFD60A");
        }

        public void RefreshStats(int ros, int profiles, int services, int quiz)
        {
            RoCount      = ros;
            ProfileCount = profiles;
            ServiceCount = services;
            QuizCount    = quiz;
            UserCount    = Users.Count;
        }

        [RelayCommand]
        private void CreateUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
            {
                ShowFeedback("Username is required.", "#FF453A");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewUserPin) || NewUserPin.Trim().Length < 4)
            {
                ShowFeedback("PIN must be at least 4 characters.", "#FF453A");
                return;
            }

            if (!Enum.TryParse<UserRole>(NewUserRole, out var role))
            {
                ShowFeedback("Select a valid role.", "#FF453A");
                return;
            }

            var linkedProfileId = role == UserRole.Advisor ? SelectedAdvisorForUser?.Id ?? string.Empty : string.Empty;
            var account = _auth.CreateUser(NewUsername, NewDisplayName, role, NewUserPin.Trim(), linkedProfileId);
            if (!_auth.AddUser(account))
            {
                ShowFeedback("Could not create user. Username may already exist.", "#FF453A");
                return;
            }

            NewUsername = string.Empty;
            NewDisplayName = string.Empty;
            NewUserPin = string.Empty;
            NewUserRole = "Advisor";
            SelectedAdvisorForUser = null;
            RefreshUsers();
            ShowFeedback("User account created.", "#30D158");
        }

        [RelayCommand]
        private void DeactivateUser(UserAccount? account)
        {
            if (account == null) return;
            if (account.Role == UserRole.Admin && Users.Count(u => u.IsActive && u.Role == UserRole.Admin) <= 1)
            {
                ShowFeedback("Cannot deactivate the last active admin.", "#FF453A");
                return;
            }

            account.IsActive = false;
            _auth.Save();
            RefreshUsers();
            ShowFeedback("User deactivated.", "#FFD60A");
        }

        private async void ShowFeedback(string msg, string color)
        {
            StatusMessage = msg;
            StatusColor   = color;
            ShowStatus    = true;
            await Task.Delay(3500);
            ShowStatus    = false;
        }

        private void RefreshUsers()
        {
            _auth.Load();
            Users = new ObservableCollection<UserAccount>(_auth.Users.OrderByDescending(u => u.IsActive).ThenBy(u => u.DisplayName));
            AdvisorProfileOptions = new ObservableCollection<AdvisorProfile>(DataLoaderService.LoadProfiles().OrderBy(p => p.Name));
            UserCount = Users.Count;
        }

        private static string FindDataFile(string filename)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            return new[]
            {
                Path.Combine(dir, filename),
                Path.Combine(dir, "Data", filename),
                Path.Combine(Directory.GetCurrentDirectory(), filename),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", filename)
            }.FirstOrDefault(File.Exists) ?? Path.Combine(dir, filename);
        }

        private static bool ConfirmDanger(string message)
            => MessageBox.Show(
                   message,
                   "Confirm destructive action",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning,
                   MessageBoxResult.No) == MessageBoxResult.Yes;
    }
}
