using System.Collections.Generic;

namespace BayBrain.Models
{
    /// <summary>
    /// Persisted profile for a single service advisor.
    /// All quiz history and aggregate stats are stored here.
    /// </summary>
    public class AdvisorProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#0A84FF";   // accent color for avatar circle
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastActiveAt { get; set; } = DateTime.Now;

        /// <summary>All completed quiz sessions.</summary>
        public List<QuizSessionRecord> SessionHistory { get; set; } = new();

        // ── Computed stats ───────────────────────────────────────────────
        public int TotalSessionsCompleted => SessionHistory.Count;
        public int TotalQuestionsAnswered => SessionHistory.Sum(s => s.TotalQuestions);
        public int TotalCorrect => SessionHistory.Sum(s => s.CorrectCount);

        public double OverallAccuracy => TotalQuestionsAnswered > 0
            ? (double)TotalCorrect / TotalQuestionsAnswered * 100 : 0;

        public double BestScore => SessionHistory.Count > 0
            ? SessionHistory.Max(s => s.ScorePercent) : 0;

        public double AverageScore => SessionHistory.Count > 0
            ? SessionHistory.Average(s => s.ScorePercent) : 0;

        /// <summary>Score trend: positive = improving, negative = declining.</summary>
        public double RecentTrend
        {
            get
            {
                if (SessionHistory.Count < 2) return 0;
                var recent = SessionHistory.TakeLast(3).Average(s => s.ScorePercent);
                var older  = SessionHistory.SkipLast(3).TakeLast(3).DefaultIfEmpty()
                                           .Average(s => s?.ScorePercent ?? 0);
                return recent - older;
            }
        }

        public string RankTitle => OverallAccuracy switch
        {
            >= 90 => "🏆 Expert Advisor",
            >= 75 => "⭐ Senior Advisor",
            >= 60 => "👍 Advisor",
            >= 40 => "📚 Apprentice",
            _     => "🔄 Trainee"
        };

        public string WeakestCategory
        {
            get
            {
                var allResults = SessionHistory
                    .SelectMany(s => s.CategoryBreakdown)
                    .GroupBy(r => r.Category)
                    .Select(g => new { Category = g.Key, Accuracy = g.Average(x => x.Accuracy) })
                    .OrderBy(x => x.Accuracy)
                    .FirstOrDefault();
                return allResults?.Category ?? "—";
            }
        }

        public string StrongestCategory
        {
            get
            {
                var allResults = SessionHistory
                    .SelectMany(s => s.CategoryBreakdown)
                    .GroupBy(r => r.Category)
                    .Select(g => new { Category = g.Key, Accuracy = g.Average(x => x.Accuracy) })
                    .OrderByDescending(x => x.Accuracy)
                    .FirstOrDefault();
                return allResults?.Category ?? "—";
            }
        }

        /// <summary>Derive initials from display name.</summary>
        public void UpdateInitials()
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : Name.Length > 0 ? Name[..Math.Min(2, Name.Length)].ToUpper() : "?";
        }
    }

    /// <summary>One completed quiz session stored in a profile.</summary>
    public class QuizSessionRecord
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public string? CategoryFilter { get; set; }   // null = all categories
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int DurationSeconds { get; set; }
        public double ScorePercent => TotalQuestions > 0
            ? (double)CorrectCount / TotalQuestions * 100 : 0;

        public List<CategoryAccuracy> CategoryBreakdown { get; set; } = new();
    }

    public class CategoryAccuracy
    {
        public string Category { get; set; } = string.Empty;
        public int Correct { get; set; }
        public int Total { get; set; }
        public double Accuracy => Total > 0 ? (double)Correct / Total * 100 : 0;
    }
}
