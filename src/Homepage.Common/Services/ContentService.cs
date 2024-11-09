using Serilog;

using System.Net.Http.Json;
using System.Text.Json;

using Homepage.Common.Models;

namespace Homepage.Common.Services;

public class ContentService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    public const string BaseUri = "Content";

    private Metadata? _metadata;
    private List<string>? _tags;
    private List<string>? _categories;
    private List<string>? _keywords;

    public async Task<Metadata> GetMetadata(string? section = null)
    {
        var logger = Log.Logger.ForContext<ContentService>();
        if (_metadata == null)
        {
            try
            {
                var path = string.IsNullOrWhiteSpace(section) ? $"{BaseUri}/metadata.json" : $"{BaseUri}/{section}/metadata.json";
                logger.Information("Loading metadata from: {0}/metadata.json", path);
                var response = await _httpClient.GetAsync($"{BaseUri}/metadata.json");
                response.EnsureSuccessStatusCode();
                logger.Information("Successful status code received from: {0}/metadata.json", path);
                _metadata = await JsonSerializer.DeserializeAsync<Metadata>(await response.Content.ReadAsStreamAsync());
                logger.Information("Metadata loaded: {0}", _metadata?.Posts.Count().ToString() ?? "null");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading metadata.");
                throw;
            }
            if (_metadata == null)
            {
                logger.Warning("Metadata is null.");
                _metadata ??= new();
            }
        }

        return _metadata!;
    }

    public async Task<TData?> GetJson<TData>(string? section = null) where TData : class
    {
        var logger = Log.Logger.ForContext<ContentService>();
        try
        {
            var path = string.IsNullOrWhiteSpace(section) ? $"{BaseUri}/metadata.json" : $"{BaseUri}/{section}/metadata.json";
            logger.Information("Loading JSON from: {0}", path);
            var response = await _httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            logger.Information("Successful status code received from: {0}", path);
            TData? result = await JsonSerializer.DeserializeAsync<TData>(await response.Content.ReadAsStreamAsync());
            logger.Information("JSON loaded: {0}", result?.ToString() ?? "null");

            return result;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error loading JSON.");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetCategories()
    {
        if (_categories == null)
        {
            var metadata = await GetMetadata();
            _categories = metadata.Posts.SelectMany(x => x.Categories).Distinct().ToList();
            Log.Logger.ForContext<ContentService>().Information("Categories loaded: {0}", _categories.Count());
        }
        return _categories;
    }

    public async Task<IEnumerable<string>> GetTags()
    {
        if (_tags == null)
        {
            var metadata = await GetMetadata();
            _tags = metadata.Posts.SelectMany(item => item.Tags).Distinct().ToList();
            Log.ForContext<ContentService>().Information("Tags loaded: {0}", _tags.Count());
        }

        return _tags;
    }

    public async Task<IEnumerable<string>> GetCategoryTags(string category)
    {
        var tags = await GetTags();
        return tags.Where(x => x.StartsWith(category));
    }

    public async Task<IEnumerable<string>> GetKeywords()
    {
        if (_keywords == null)
        {
            var metadata = await GetMetadata();
            _keywords = metadata.Posts.SelectMany(x => x.Keywords).Distinct().ToList();
            Log.ForContext<ContentService>().Information("Keywords loaded: {0}", _keywords.Count());
        }

        return _keywords;
    }

    public async Task<IEnumerable<string>> GetCategoryKeywords(string category)
    {
        var keywords = await GetKeywords();
        return keywords.Where(x => x.StartsWith(category));
    }

    public async Task<IEnumerable<PostMetadata>> GetContentByCategory(string category)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post.Categories.Contains(category));
    }

    public async Task<IEnumerable<PostMetadata>> GetContentByTagAsync(string tag)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post.Tags.Contains(tag));
    }

    public async Task<IEnumerable<PostMetadata>> GetRelatedContent(PostMetadata currentItem)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post != currentItem && post.Tags.Any(tag => currentItem.Tags.Contains(tag)));
    }
}