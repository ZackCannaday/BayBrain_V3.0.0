using BayBrain.Models;

namespace BayBrain.Services
{
    /// <summary>
    /// Calculates a dynamic urgency score based on service base urgency
    /// plus contextual modifiers (mileage overdue, customer symptoms, season).
    /// </summary>
    public class UrgencyService
    {
        public UrgencyScore Calculate(ServiceItem service,
                                     int milesOverdue = 0,
                                     bool hasSymptoms = false,
                                     bool isHighMileageVehicle = false)
        {
            int score = service.BaseUrgency;

            // Mileage overdue modifier
            if (milesOverdue > 10000) score = Math.Min(10, score + 3);
            else if (milesOverdue > 5000) score = Math.Min(10, score + 2);
            else if (milesOverdue > 2000) score = Math.Min(10, score + 1);

            // Active symptom modifier — customer reports an issue
            if (hasSymptoms) score = Math.Min(10, score + 2);

            // High-mileage vehicle modifier
            if (isHighMileageVehicle) score = Math.Min(10, score + 1);

            string reasoning = BuildReasoning(service, milesOverdue, hasSymptoms, isHighMileageVehicle, score);
            return UrgencyScore.FromScore(score, reasoning);
        }

        private static string BuildReasoning(ServiceItem service, int milesOverdue,
                                              bool hasSymptoms, bool isHighMileage, int finalScore)
        {
            var parts = new System.Collections.Generic.List<string>
            {
                $"Base urgency for {service.Name}: {service.BaseUrgency}/10"
            };

            if (milesOverdue > 0)
                parts.Add($"+{(milesOverdue > 10000 ? 3 : milesOverdue > 5000 ? 2 : 1)} for being {milesOverdue:N0} miles overdue");
            if (hasSymptoms)
                parts.Add("+2 for active customer-reported symptoms");
            if (isHighMileage)
                parts.Add("+1 for high-mileage vehicle");

            parts.Add($"Final score: {finalScore}/10");
            return string.Join(" · ", parts);
        }

        /// <summary>Returns urgency for display without modifiers.</summary>
        public UrgencyScore GetBaseUrgency(ServiceItem service)
            => UrgencyScore.FromScore(service.BaseUrgency,
                $"Standard urgency for {service.Name}: {service.BaseUrgency}/10");
    }
}
