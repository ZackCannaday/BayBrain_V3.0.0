using BayBrain.Models;
using System.Text;

namespace BayBrain.Services
{
    /// <summary>
    /// Generates personalized customer-facing sales scripts for service advisors.
    /// </summary>
    public class ScriptGeneratorService
    {
        public string Generate(ServiceItem service,
                               UrgencyScore urgency,
                               string customerName = "your vehicle",
                               bool hasSymptoms = false,
                               string symptomDescription = "")
        {
            var sb = new StringBuilder();
            string vehicleRef = string.IsNullOrWhiteSpace(customerName) ? "your vehicle" : customerName;

            sb.AppendLine(BuildOpener(service, vehicleRef, urgency));
            sb.AppendLine();
            sb.AppendLine(BuildExplanation(service));
            sb.AppendLine();
            sb.AppendLine(BuildConsequence(service, urgency));
            sb.AppendLine();
            sb.AppendLine(BuildValueProp(service));
            sb.AppendLine();
            sb.AppendLine(BuildClose(service, urgency));

            return sb.ToString().Trim();
        }

        private static string BuildOpener(ServiceItem service, string vehicleRef, UrgencyScore urgency)
        {
            return urgency.Level switch
            {
                UrgencyLevel.Critical =>
                    $"I want to make sure you're aware of something important for {vehicleRef} today — " +
                    $"your {service.Name} is at a critical point and really shouldn't be put off any longer.",

                UrgencyLevel.High =>
                    $"While we had {vehicleRef} in today, we noticed your {service.Name} is coming due — " +
                    $"and given where things stand, I'd recommend we take care of it now.",

                UrgencyLevel.Medium =>
                    $"One thing I wanted to mention for {vehicleRef} is the {service.Name}. " +
                    $"It's coming up on its service interval, and today is actually a great time to get ahead of it.",

                _ =>
                    $"Something to keep on your radar for {vehicleRef} — the {service.Name} will be coming due soon. " +
                    $"No rush today, but worth planning for."
            };
        }

        private static string BuildExplanation(ServiceItem service)
        {
            return $"Here's what this service does: {service.ShortDescription} " +
                   $"{service.WhyItMatters}";
        }

        private static string BuildConsequence(ServiceItem service, UrgencyScore urgency)
        {
            if (urgency.Level >= UrgencyLevel.High)
            {
                return $"If we put this off, what typically happens is: {service.SkipConsequence} " +
                       $"And that's exactly the situation this service is designed to prevent.";
            }
            return $"Staying on top of this prevents: {service.SkipConsequence}";
        }

        private static string BuildValueProp(ServiceItem service)
        {
            if (!string.IsNullOrEmpty(service.PriceRange))
            {
                return $"Today the {service.Name} runs {service.PriceRange} and takes about " +
                       $"{FormatTime(service.EstimatedMinutes)} — so it's very manageable while you're already here.";
            }
            return $"This takes approximately {FormatTime(service.EstimatedMinutes)} and we can work it in today.";
        }

        private static string BuildClose(ServiceItem service, UrgencyScore urgency)
        {
            return urgency.Level switch
            {
                UrgencyLevel.Critical =>
                    $"I really want to make sure we get this taken care of for you today. Can I go ahead and add it to your work order?",

                UrgencyLevel.High =>
                    $"Would you like me to go ahead and get that scheduled while we have your vehicle here?",

                UrgencyLevel.Medium =>
                    $"Would you like to take care of it today, or would you prefer we schedule it for your next visit?",

                _ =>
                    $"Just something to keep in mind — I can note it in your file so we flag it next time you're in."
            };
        }

        private static string FormatTime(int minutes)
        {
            if (minutes < 60) return $"{minutes} minutes";
            int h = minutes / 60;
            int m = minutes % 60;
            return m == 0 ? $"{h} hour{(h > 1 ? "s" : "")}"
                          : $"{h} hour{(h > 1 ? "s" : "")} {m} minutes";
        }
    }
}
