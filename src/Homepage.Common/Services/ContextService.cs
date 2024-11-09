using System.Net.Http.Json;

using Homepage.Common.Models;

namespace Homepage.Common.Services;

public class ContextService(ContentService contentService, Similarity jaccardSimilarity)
{
    private readonly ContentService _contentService = contentService;
    public const string BaseUri = "Content/";

    public IEnumerable<PostMetadata> Metadata
    {
        get
        {
            if (_metadata == null)
            {
                GetMetadata().Wait();
            }
            return _metadata!;
        }
    }
    private IEnumerable<PostMetadata>? _metadata;

    public IEnumerable<string> Tags
    {
        get
        {
            if (_tags == null)
            {
                _tags = Metadata.SelectMany(m => m.Tags).Distinct().ToList();
                SortTags();
            }
            return _tags!;
        }
    }
    private List<string>? _tags;
    private void SortTags() => throw new NotImplementedException();

    public IEnumerable<string> Categories
    {
        get
        {
            if (_categories == null)
            {
                _categories = Metadata.SelectMany(m => m.Categories).Distinct().ToList();
                SortCategories();
            }
            return _categories!;
        }
    }
    private List<string>? _categories;

    public IEnumerable<string> Keywords
    {
        get
        {
            if (_keywords == null)
            {
                _keywords = Metadata?.SelectMany(m => m.Keywords).Distinct().ToList();
                SortKeywords();
            }
            return _keywords!;
        }
    }
    private IEnumerable<string>? _keywords;
    private void SortKeywords() => throw new NotImplementedException();


    private async Task<List<PostMetadata>> GetMetadata()
    {
        try
        {
            var response = await _contentService.GetJson<List<PostMetadata>>();
            _metadata = response ?? new List<PostMetadata>();
            return _metadata.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new List<PostMetadata>();
        }
    }

    private void SortCategories()
    {
        ArgumentNullException.ThrowIfNull(_categories);

        // 1. Collect data on each category and its connected tags and keywords. That is all tags and keywords that are connected to the posts in that category.
        var categoryData = new Dictionary<string, HashSet<string>>();

        foreach (var category in _categories)
        {
            var items = new HashSet<string>();

            foreach (var post in Metadata.Where(p => p.Categories.Contains(category)))
            {
                items.UnionWith(post.Tags);
                items.UnionWith(post.Keywords);
            }

            categoryData.Add(category, items);
        }

        // 2. Determine a base category to start with. For example, DevOps.
        var baseCategory = "DevOps";
        var baseCategoryData = categoryData[baseCategory];

        if (baseCategoryData == default)
        {
            baseCategory = _categories.FirstOrDefault() ?? throw new InvalidOperationException("No categories found.");
            baseCategoryData = categoryData[baseCategory];
        }

        // 3. Calculate the Jaccard similarity between the base category and all other categories to determine the sort order.
        // 4. Sort the categories according to the Jaccard similarity.
        _categories = _categories.OrderByDescending(c => Similarity.CalculateJaccard(baseCategoryData, categoryData[c])).ToList();
    }
}