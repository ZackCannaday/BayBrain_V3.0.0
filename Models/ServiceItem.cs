using System.Collections.Generic;

namespace BayBrain.Models
{
    /// <summary>
    /// Represents an automotive service offered at the dealership.
    /// </summary>
    public class ServiceItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        /// <summary>Simple customer-friendly description (1-2 sentences).</summary>
        public string ShortDescription { get; set; } = string.Empty;

        /// <summary>Full technical explanation for advisor reference.</summary>
        public string DetailedDescription { get; set; } = string.Empty;

        /// <summary>Why this service matters; used in customer script.</summary>
        public string WhyItMatters { get; set; } = string.Empty;

        /// <summary>Typical mileage interval or time trigger.</summary>
        public string Interval { get; set; } = string.Empty;

        /// <summary>Consequence of not performing the service.</summary>
        public string SkipConsequence { get; set; } = string.Empty;

        /// <summary>Base urgency score 1-10 set by data layer.</summary>
        public int BaseUrgency { get; set; }

        /// <summary>Tags used for fuzzy search matching.</summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>Approximate price range string, e.g. "$29–$49".</summary>
        public string PriceRange { get; set; } = string.Empty;

        /// <summary>Average completion time in minutes.</summary>
        public int EstimatedMinutes { get; set; }
    }
}
