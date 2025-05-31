using Homepage.Common.Helpers;
using Homepage.Common.Models;

namespace Homepage.Common.Services
{
    public class ContextService
    {
        private readonly ContentMarkdownService _contentService;
        private readonly Similarity _jaccardSimilarity;

        public ContextService(ContentMarkdownService contentService, Similarity jaccardSimilarity)
        {
            _contentService = contentService;
            _jaccardSimilarity = jaccardSimilarity;
        }

        public async Task<List<ContentMetadata>> GetAllContentMetadataAsync()
        {
            return await _contentService.GetContentMetadataAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var categories = await _contentService.GetCategories();
            return await SortCategories(categories.ToList());
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            var tags = await _contentService.GetTags();
            return SortTags(tags.ToList());
        }

        public async Task<IEnumerable<string>> GetKeywordsAsync()
        {
            var keywords = await _contentService.GetKeywords();
            return SortKeywords(keywords.ToList());
        }

        private async Task<List<string>> SortCategories(List<string> categories)
        {
            if (!categories.Any()) return categories;

            var categoryData = new Dictionary<string, HashSet<string>>();
            var allContentMetadata = await GetAllContentMetadataAsync();

            foreach (var category in categories)
            {
                var items = new HashSet<string>();
                var contentForCategory = allContentMetadata.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));

                foreach (var post in contentForCategory)
                {
                    if (post.Tags != null) items.UnionWith(post.Tags);
                    if (post.Keywords != null) items.UnionWith(post.Keywords);
                }

                categoryData.Add(category, items);
            }

            string baseCategory = "DevOps";
            if (!categoryData.ContainsKey(baseCategory) && categories.Any())
            {
                baseCategory = categories.First();
            }

            if (categoryData.TryGetValue(baseCategory, out var baseCategoryTags))
            {
                return categories.OrderByDescending(c => Similarity.CalculateJaccard(baseCategoryTags, categoryData[c])).ToList();
            }

            return categories.OrderBy(c => c).ToList();
        }

        private List<string> SortTags(List<string> tags)
        {
            return tags.OrderBy(t => t).ToList();
        }

        private List<string> SortKeywords(List<string> keywords)
        {
            return keywords.OrderBy(k => k).ToList();
        }
    }
}
