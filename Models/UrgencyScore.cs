namespace BayBrain.Models
{
    public enum UrgencyLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class UrgencyScore
    {
        public int Score { get; set; }          // 1–10
        public UrgencyLevel Level { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;

        public static UrgencyScore FromScore(int score, string reasoning = "")
        {
            return new UrgencyScore
            {
                Score = Math.Clamp(score, 1, 10),
                Level = score switch
                {
                    >= 9 => UrgencyLevel.Critical,
                    >= 7 => UrgencyLevel.High,
                    >= 4 => UrgencyLevel.Medium,
                    _ => UrgencyLevel.Low
                },
                Label = score switch
                {
                    >= 9 => "CRITICAL",
                    >= 7 => "HIGH",
                    >= 4 => "MODERATE",
                    _ => "LOW"
                },
                ColorHex = score switch
                {
                    >= 9 => "#FF3B30",
                    >= 7 => "#FF9500",
                    >= 4 => "#FFCC00",
                    _ => "#34C759"
                },
                Reasoning = reasoning
            };
        }
    }
}
