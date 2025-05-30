using Homepage.Common.Helpers;
using Homepage.Common.Models;

namespace Homepage.Common.Services
{
    // ContextService is now primarily responsible for providing derived, sorted lists
    // of content metadata, categories, tags, and keywords based on the ContentService.
    public class ContextService(ContentService contentService, Similarity jaccardSimilarity)
    {
        private readonly ContentService _contentService = contentService;
        private readonly Similarity _jaccardSimilarity = jaccardSimilarity; // Inject Similarity

        // This property now directly gets content metadata from ContentService
        public async Task<List<ContentMetadata>> GetAllContentMetadataAsync()
        {
            return await _contentService.GetContentMetadataAsync();
        }

        // Properties for derived lists, now calling the ContentService
        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var categories = await _contentService.GetCategories();
            return SortCategories(categories.ToList()); // Sort categories
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            var tags = await _contentService.GetTags();
            return SortTags(tags.ToList()); // Sort tags
        }

        public async Task<IEnumerable<string>> GetKeywordsAsync()
        {
            var keywords = await _contentService.GetKeywords();
            return SortKeywords(keywords.ToList()); // Sort keywords
        }

        // --- Sorting Methods ---
        // These methods now operate on the derived lists.
        // The previous complex similarity sorting for categories is kept for now,
        // but can be simplified if explicit audience filtering makes it less relevant.
        private List<string> SortCategories(List<string> categories)
        {
            if (!categories.Any()) return categories;

            // This logic assumes a "baseCategory" for similarity comparison.
            // If this is no longer desired with explicit audience filtering,
            // this can be simplified to alphabetical sorting.
            var categoryData = new Dictionary<string, HashSet<string>>();
            foreach (var category in categories)
            {
                var items = new HashSet<string>();
                var contentForCategory = GetAllContentMetadataAsync().Result.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));

                foreach (var post in contentForCategory)
                {
                    items.UnionWith(post.Tags);
                    items.UnionWith(post.Keywords ?? new List<string>());
                }

                categoryData.Add(category, items);
            }

            string baseCategory = "DevOps"; // Default base category for sorting
            if (!categoryData.ContainsKey(baseCategory) && categories.Any())
            {
                baseCategory = categories.First(); // Fallback to first category if DevOps not found
            }

            if (categoryData.TryGetValue(baseCategory, out var baseCategoryTags))
            {
                return categories.OrderByDescending(c => Similarity.CalculateJaccard(baseCategoryTags, categoryData[c])).ToList();
            }

            return categories.OrderBy(c => c).ToList();
        }

        private List<string> SortTags(List<string> tags)
        {
            // Simple alphabetical sorting for tags
            return tags.OrderBy(t => t).ToList();
        }

        private List<string> SortKeywords(List<string> keywords)
        {
            // Simple alphabetical sorting for keywords
            return keywords.OrderBy(k => k).ToList();
        }
    }
}
