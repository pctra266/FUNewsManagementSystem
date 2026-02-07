using DataAccess.Repositories;
using System.Text.RegularExpressions;

namespace BussinessLogic.Services
{
    public class AIService : IAIService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AIService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dictionary<string, double>> SuggestTagsAsync(string content)
        {
            var suggestions = new Dictionary<string, double>();
            if (string.IsNullOrWhiteSpace(content)) return suggestions;

            // 1. Keyword Extraction (Simple Simulation)
            // Remove special chars and split
            var words = Regex.Replace(content.ToLower(), @"[^a-z0-9\s]", "")
                             .Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Length > 3) // Filter short words
                             .Where(w => !StopWords.Contains(w)) // Filter stop words
                             .GroupBy(w => w)
                             .OrderByDescending(g => g.Count())
                             .Take(5)
                             .Select(g => g.Key)
                             .ToList();

            // Assign initial confidence to extracted keywords
            foreach (var word in words)
            {
                // Simple confidence: 0.8 descending
                suggestions[word] = 0.8; 
            }

            // 2. Learning Cache (Popular Tags)
            // Get top 5 popular tags from DB
            var popularTags = await _unitOfWork.TagRepository.GetMostPopularTagsAsync(5);
            
            foreach (var tag in popularTags)
            {
                if (tag.TagName != null)
                {
                    string tagNameLower = tag.TagName.ToLower();
                    
                    // If the content explicitly contains the popular tag name, boost it
                    if (content.Contains(tagNameLower, StringComparison.OrdinalIgnoreCase))
                    {
                        if (suggestions.ContainsKey(tagNameLower))
                            suggestions[tagNameLower] = 0.95; // Boost existing
                        else
                            suggestions[tagNameLower] = 0.9; // New high confidence match
                    }
                    else if (!suggestions.ContainsKey(tagNameLower))
                    {
                        // Suggest popular tag with lower confidence just because it's popular
                        suggestions[tagNameLower] = 0.4;
                    }
                }
            }

            return suggestions.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "the", "be", "to", "of", "and", "a", "in", "that", "have", "i", "it", "for", "not", "on", "with", "he", "as", "you", "do", "at", 
            "this", "but", "his", "by", "from", "they", "we", "say", "her", "she", "or", "an", "will", "my", "one", "all", "would", "there", 
            "their", "what", "so", "up", "out", "if", "about", "who", "get", "which", "go", "me", "when", "make", "can", "like", "time", "no", 
            "just", "him", "know", "take", "people", "into", "year", "your", "good", "some", "could", "them", "see", "other", "than", "then", 
            "now", "look", "only", "come", "its", "over", "think", "also", "back", "after", "use", "two", "how", "our", "work", "first", "well", 
            "way", "even", "new", "want", "because", "any", "these", "give", "day", "most", "us"
        };
    }
}
