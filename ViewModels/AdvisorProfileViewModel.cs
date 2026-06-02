using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BayBrain.ViewModels
{
    public partial class AdvisorProfileViewModel : ObservableObject
    {
        private List<AdvisorProfile> _allProfiles = new();

        [ObservableProperty] private ObservableCollection<AdvisorProfile> _profiles = new();
        [ObservableProperty] private AdvisorProfile? _selectedProfile;
        [ObservableProperty] private AdvisorProfile? _activeAdvisor;
        [ObservableProperty] private bool _isCreatingNew = false;
        [ObservableProperty] private string _newAdvisorName = string.Empty;
        [ObservableProperty] private string _newAdvisorColor = "#0A84FF";
        [ObservableProperty] private bool _hasProfiles = false;
        [ObservableProperty] private bool _showLeaderboard = false;
        [ObservableProperty] private ObservableCollection<LeaderboardRow> _leaderboard = new();
        [ObservableProperty] private ObservableCollection<SessionHistoryRow> _sessionHistory = new();

        public Action<AdvisorProfile?>? ActiveAdvisorChanged { get; set; }
        public Func<AdvisorProfile, bool>? CanActivateProfile { get; set; }
        public Action<string>? ActivationDenied { get; set; }

        public static readonly string[] AvatarColors =
        {
            "#0A84FF", "#30D158", "#FF9F0A", "#FF453A",
            "#BF5AF2", "#64D2FF", "#FF6B6B", "#FFD700"
        };

        public AdvisorProfileViewModel()
        {
            Load();
        }

        public void Load()
        {
            _allProfiles = DataLoaderService.LoadProfiles();
            RefreshProfiles();
        }

        private void RefreshProfiles()
        {
            Profiles = new ObservableCollection<AdvisorProfile>(
                _allProfiles.OrderByDescending(p => p.LastActiveAt));
            HasProfiles = Profiles.Count > 0;
            RefreshLeaderboard();
        }

        private void RefreshLeaderboard()
        {
            var rows = _allProfiles
                .Where(p => p.TotalSessionsCompleted > 0)
                .OrderByDescending(p => p.OverallAccuracy)
                .Select((p, i) => new LeaderboardRow
                {
                    Rank = i + 1,
                    Profile = p,
                    MedallionColor = i switch { 0 => "#FFD700", 1 => "#C0C0C0", 2 => "#CD7F32", _ => "#3A3A3C" }
                })
                .ToList();
            Leaderboard = new ObservableCollection<LeaderboardRow>(rows);
        }

        private void RefreshSessionHistory()
        {
            if (SelectedProfile == null) { SessionHistory.Clear(); return; }
            var rows = SelectedProfile.SessionHistory
                .OrderByDescending(s => s.CompletedAt)
                .Take(20)
                .Select((s, i) => new SessionHistoryRow
                {
                    Index = i + 1,
                    Session = s,
                    ScoreLabel = $"{s.CorrectCount}/{s.TotalQuestions}  ({s.ScorePercent:F0}%)",
                    DateLabel = s.CompletedAt.ToString("MMM d, yyyy  h:mm tt"),
                    CategoryLabel = s.CategoryFilter ?? "All Categories",
                    ScoreColor = s.ScorePercent switch
                    {
                        >= 90 => "#30D158",
                        >= 75 => "#30D158",
                        >= 60 => "#FFD700",
                        >= 40 => "#FF9F0A",
                        _     => "#FF453A"
                    }
                });
            SessionHistory = new ObservableCollection<SessionHistoryRow>(rows);
        }

        partial void OnSelectedProfileChanged(AdvisorProfile? value) => RefreshSessionHistory();

        // ── Commands ─────────────────────────────────────────────────────

        [RelayCommand]
        private void StartCreatingNew()
        {
            NewAdvisorName = string.Empty;
            NewAdvisorColor = AvatarColors[_allProfiles.Count % AvatarColors.Length];
            IsCreatingNew = true;
        }

        [RelayCommand]
        private void CancelNew() => IsCreatingNew = false;

        [RelayCommand]
        private void CreateProfile()
        {
            if (string.IsNullOrWhiteSpace(NewAdvisorName)) return;
            var profile = new AdvisorProfile
            {
                Name = NewAdvisorName.Trim(),
                AvatarColor = NewAdvisorColor
            };
            profile.UpdateInitials();
            _allProfiles.Add(profile);
            DataLoaderService.SaveProfiles(_allProfiles);
            RefreshProfiles();
            SelectedProfile = profile;
            IsCreatingNew = false;
        }

        [RelayCommand]
        private void DeleteProfile(AdvisorProfile? profile)
        {
            if (profile == null) return;
            _allProfiles.Remove(profile);
            DataLoaderService.SaveProfiles(_allProfiles);
            if (SelectedProfile == profile) SelectedProfile = null;
            if (ActiveAdvisor == profile) ActiveAdvisor = null;
            RefreshProfiles();
        }

        [RelayCommand]
        private void ViewProfile(AdvisorProfile? profile)
        {
            if (profile == null) return;
            SelectedProfile = profile;
        }

        [RelayCommand]
        private void ActivateAdvisor(AdvisorProfile? profile)
        {
            if (profile == null) return;
            if (CanActivateProfile != null && !CanActivateProfile(profile))
            {
                ActivationDenied?.Invoke("Only admins/managers can switch advisors. Advisor users stay tied to their login account.");
                return;
            }

            ActiveAdvisor = profile;
            SelectedProfile = profile;
            profile.LastActiveAt = DateTime.Now;
            DataLoaderService.SaveProfiles(_allProfiles);
            ActiveAdvisorChanged?.Invoke(profile);
        }

        public AdvisorProfile? FindProfileById(string? id)
            => string.IsNullOrWhiteSpace(id) ? null : _allProfiles.FirstOrDefault(p => p.Id == id);

        public void SetActiveAdvisorById(string? id)
        {
            var profile = FindProfileById(id);
            ActiveAdvisor = profile;
            SelectedProfile = profile;
            ActiveAdvisorChanged?.Invoke(profile);
        }

        [RelayCommand]
        private void ToggleLeaderboard() => ShowLeaderboard = !ShowLeaderboard;

        [RelayCommand]
        private void SetColorForNew(string color) => NewAdvisorColor = color;

        public void RecordSession(QuizSessionRecord record)
        {
            if (ActiveAdvisor == null) return;
            ActiveAdvisor.SessionHistory.Add(record);
            ActiveAdvisor.LastActiveAt = DateTime.Now;
            DataLoaderService.SaveProfiles(_allProfiles);
            RefreshProfiles();
            RefreshSessionHistory();
        }
    }

    public class LeaderboardRow
    {
        public int Rank { get; set; }
        public AdvisorProfile Profile { get; set; } = null!;
        public string MedallionColor { get; set; } = "#3A3A3C";
        public string RankDisplay => Rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{Rank}" };
    }

    public class SessionHistoryRow
    {
        public int Index { get; set; }
        public QuizSessionRecord Session { get; set; } = null!;
        public string ScoreLabel { get; set; } = string.Empty;
        public string DateLabel { get; set; } = string.Empty;
        public string CategoryLabel { get; set; } = string.Empty;
        public string ScoreColor { get; set; } = "#8E8E93";
    }
}
