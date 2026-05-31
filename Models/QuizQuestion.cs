using System.Collections.Generic;

namespace BayBrain.Models
{
    public class QuizQuestion
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string RelatedServiceId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int DifficultyLevel { get; set; } = 1; // 1=Easy, 2=Medium, 3=Hard
    }

    public class QuizSession
    {
        public List<QuizQuestion> Questions { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
        public int CorrectCount { get; set; } = 0;
        public List<QuizAnswer> Answers { get; set; } = new();

        public bool IsComplete => CurrentIndex >= Questions.Count;
        public double ScorePercent => Questions.Count > 0
            ? (double)CorrectCount / Questions.Count * 100 : 0;
    }

    public class QuizAnswer
    {
        public string QuestionId { get; set; } = string.Empty;
        public int SelectedIndex { get; set; }
        public bool IsCorrect { get; set; }
        public int TimeTakenSeconds { get; set; }
    }
}
