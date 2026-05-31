using BayBrain.Models;
using BayBrain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BayBrain.ViewModels
{
    public partial class QuizViewModel : ObservableObject
    {
        private QuizSession _session = new();
        private List<QuizQuestion> _allQuestions = new();
        private Stopwatch _sessionTimer = new();

        // ── Category filter ──────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<string> _availableCategories = new();
        [ObservableProperty] private string? _selectedCategory = null;   // null = all
        [ObservableProperty] private int _questionCount = 10;

        // ── Quiz state ───────────────────────────────────────────────────
        [ObservableProperty] private QuizQuestion? _currentQuestion;
        [ObservableProperty] private int _currentIndex = 0;
        [ObservableProperty] private int _totalQuestions = 0;
        [ObservableProperty] private int _correctCount = 0;
        [ObservableProperty] private int? _selectedAnswerIndex = null;
        [ObservableProperty] private bool _hasAnswered = false;
        [ObservableProperty] private bool _isComplete = false;
        [ObservableProperty] private bool _isInProgress = false;
        [ObservableProperty] private string _feedbackText = string.Empty;
        [ObservableProperty] private bool _lastAnswerCorrect = false;
        [ObservableProperty] private double _scorePercent = 0;
        [ObservableProperty] private string _scoreGrade = string.Empty;
        [ObservableProperty] private ObservableCollection<QuizResultRow> _resultRows = new();
        [ObservableProperty] private int _progressBarWidth = 0;   // 0–600 px

        // Callback so MainViewModel can pass the record to AdvisorProfileViewModel
        public Action<QuizSessionRecord>? OnSessionCompleted { get; set; }

        public QuizViewModel()
        {
            _allQuestions = DataLoaderService.LoadQuizQuestions();
            PopulateCategories();
        }

        private void PopulateCategories()
        {
            AvailableCategories.Clear();
            AvailableCategories.Add("All Categories");
            foreach (var cat in _allQuestions.Select(q => q.Category).Distinct().OrderBy(c => c))
                AvailableCategories.Add(cat);
        }

        public void StartNewSession(string? category = null)
        {
            SelectedCategory = category;
            var pool = _allQuestions.AsEnumerable();

            if (!string.IsNullOrEmpty(category) && category != "All Categories")
                pool = pool.Where(q => q.Category == category);

            var rng = new Random();
            var questions = pool.OrderBy(_ => rng.Next())
                                .Take(Math.Min(QuestionCount, pool.Count()))
                                .ToList();

            _session = new QuizSession { Questions = questions };
            TotalQuestions = questions.Count;
            CurrentIndex = 0;
            CorrectCount = 0;
            SelectedAnswerIndex = null;
            HasAnswered = false;
            IsComplete = false;
            IsInProgress = true;
            FeedbackText = string.Empty;
            ResultRows.Clear();
            ScorePercent = 0;
            ProgressBarWidth = 0;
            _sessionTimer.Restart();

            LoadCurrentQuestion();
        }

        private void LoadCurrentQuestion()
        {
            if (_session.IsComplete)
            {
                FinishQuiz();
                return;
            }
            CurrentQuestion = _session.Questions[_session.CurrentIndex];
            CurrentIndex = _session.CurrentIndex + 1;
            SelectedAnswerIndex = null;
            HasAnswered = false;
            FeedbackText = string.Empty;
        }

        [RelayCommand]
        private void SelectAnswer(string indexStr)
        {
            if (HasAnswered || CurrentQuestion == null) return;
            if (!int.TryParse(indexStr, out int idx)) return;

            SelectedAnswerIndex = idx;
            HasAnswered = true;
            bool correct = idx == CurrentQuestion.CorrectIndex;
            LastAnswerCorrect = correct;

            if (correct)
            {
                CorrectCount++;
                FeedbackText = "✓ Correct! " + CurrentQuestion.Explanation;
            }
            else
            {
                FeedbackText = $"✗ Not quite. Correct: \"{CurrentQuestion.Options[CurrentQuestion.CorrectIndex]}\"\n\n{CurrentQuestion.Explanation}";
            }

            _session.Answers.Add(new QuizAnswer
            {
                QuestionId = CurrentQuestion.Id,
                SelectedIndex = idx,
                IsCorrect = correct
            });

            UpdateProgress();
        }

        [RelayCommand]
        private void NextQuestion()
        {
            if (!HasAnswered) return;
            _session.CurrentIndex++;
            LoadCurrentQuestion();
        }

        private void UpdateProgress()
        {
            if (TotalQuestions > 0)
                ProgressBarWidth = (int)((double)CurrentIndex / TotalQuestions * 600);
        }

        private void FinishQuiz()
        {
            _sessionTimer.Stop();
            IsInProgress = false;
            IsComplete = true;
            ScorePercent = _session.ScorePercent;
            _session.CorrectCount = CorrectCount;

            ScoreGrade = ScorePercent switch
            {
                >= 90 => "🏆 Expert Advisor",
                >= 75 => "⭐ Strong Performance",
                >= 60 => "👍 Good Effort",
                >= 40 => "📚 Keep Studying",
                _     => "🔄 Review & Retry"
            };

            ResultRows.Clear();
            for (int i = 0; i < _session.Questions.Count; i++)
            {
                var q = _session.Questions[i];
                var a = _session.Answers.ElementAtOrDefault(i);
                ResultRows.Add(new QuizResultRow
                {
                    Number = i + 1,
                    Question = q.Question.Length > 75 ? q.Question[..72] + "…" : q.Question,
                    IsCorrect = a?.IsCorrect ?? false,
                    CorrectAnswer = q.Options[q.CorrectIndex],
                    Category = q.Category
                });
            }

            // Build session record for advisor profile
            var categoryBreakdown = _session.Questions
                .GroupBy(q => q.Category)
                .Select(g =>
                {
                    var answered = _session.Answers
                        .Join(_session.Questions.Where(q => q.Category == g.Key),
                              a => a.QuestionId, q => q.Id, (a, _) => a)
                        .ToList();
                    return new CategoryAccuracy
                    {
                        Category = g.Key,
                        Correct = answered.Count(a => a.IsCorrect),
                        Total = answered.Count
                    };
                }).ToList();

            var record = new QuizSessionRecord
            {
                TotalQuestions = TotalQuestions,
                CorrectCount = CorrectCount,
                DurationSeconds = (int)_sessionTimer.Elapsed.TotalSeconds,
                CategoryFilter = SelectedCategory == "All Categories" ? null : SelectedCategory,
                CategoryBreakdown = categoryBreakdown
            };

            OnSessionCompleted?.Invoke(record);
        }

        [RelayCommand]
        private void RestartQuiz() => StartNewSession(SelectedCategory == "All Categories" ? null : SelectedCategory);

        [RelayCommand]
        private void StartWithCategory(string? cat) => StartNewSession(cat == "All Categories" ? null : cat);
    }

    public class QuizResultRow
    {
        public int Number { get; set; }
        public string Question { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string StatusIcon => IsCorrect ? "✓" : "✗";
        public string StatusColor => IsCorrect ? "#34C759" : "#FF3B30";
    }
}
