using BayBrain.Models;
using System.Collections.Generic;
using System.Linq;

namespace BayBrain.Services
{
    /// <summary>
    /// Fuzzy keyword search across the service catalog.
    /// Accepts an injected list so data comes from JSON at runtime.
    /// </summary>
    public class SearchService
    {
        private readonly List<ServiceItem> _catalog;

        /// <summary>Primary constructor — inject a loaded list from DataLoaderService.</summary>
        public SearchService(List<ServiceItem> catalog)
        {
            _catalog = catalog;
        }

        /// <summary>Fallback: loads from built-in static database.</summary>
        public SearchService() : this(Data.ServiceDatabase.GetAll()) { }

        public List<ServiceItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _catalog.OrderBy(s => s.Category).ThenBy(s => s.Name).ToList();

            var terms = query.ToLowerInvariant()
                             .Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            return _catalog
                .Select(item => new { Item = item, Score = ScoreItem(item, terms) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Item)
                .ToList();
        }

        public List<ServiceItem> GetByCategory(string category)
            => _catalog.Where(s => s.Category == category).ToList();

        public List<string> GetCategories()
            => _catalog.Select(s => s.Category).Distinct().OrderBy(c => c).ToList();

        public ServiceItem? GetById(string id)
            => _catalog.FirstOrDefault(s => s.Id == id);

        public int TotalCount => _catalog.Count;

        /// <summary>Returns the full catalog — used by Dashboard for count stats.</summary>
        public List<ServiceItem> GetAllServices() => _catalog.ToList();

        private static int ScoreItem(ServiceItem item, string[] terms)
        {
            int score = 0;
            foreach (var term in terms)
            {
                if (item.Name.ToLower().Contains(term))             score += 10;
                if (item.Category.ToLower().Contains(term))         score += 6;
                if (item.Tags.Any(t => t.ToLower().Contains(term))) score += 8;
                if (item.ShortDescription.ToLower().Contains(term)) score += 3;
                if (item.DetailedDescription.ToLower().Contains(term)) score += 2;
                if (item.SkipConsequence.ToLower().Contains(term))  score += 1;
                if (item.WhyItMatters.ToLower().Contains(term))     score += 1;
            }
            return score;
        }
    }
}
