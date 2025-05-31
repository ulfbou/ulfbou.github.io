using System.Text.Json;

using Blazored.LocalStorage;

using Homepage.Common.Models;

using Markdig;

using Serilog;

namespace Homepage.Common.Services
{
    /// <summary>A service for managing Markdown content.</summary>
    public class ContentMarkdownService : IMarkdownService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage; // Made non-nullable as it's required for caching
        private List<ContentMetadata>? _cachedMetadata; // For content_metadata.json caching
        private readonly ILogger _logger; // Use Serilog ILogger for consistency

#if DEBUG
        public const string GITHUB_CONTENT_BASE_URL = ""; // For local debugging, might be empty or specific local URL
#else
        // IMPORTANT: Ensure this matches where your static generator places content/metadata.json and .md files
        public const string GITHUB_CONTENT_BASE_URL = ContentService.GITHUB_CONTENT_BASE_URL;
#endif

        /// <summary>Initializes a new instance of the <see cref="ContentMarkdownService"/> class.</summary>
        /// <param name="httpClient">The HTTP client used to fetch content.</param>
        /// <param name="localStorage">The local storage service used for caching.</param>
        public ContentMarkdownService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _logger = Log.Logger.ForContext<ContentMarkdownService>();
        }

        /// <inheritdoc />
        public async Task<List<ContentMetadata>> GetContentMetadataAsync()
        {
            if (_cachedMetadata != null)
            {
                _ = FetchAndCacheMetadataFromNetworkAsync();
                return _cachedMetadata;
            }
            try
            {
                var cachedJson = await _localStorage.GetItemAsync<string>("content_metadata");
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    _cachedMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(cachedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _logger.Information("Loaded metadata from local storage.");
                    _ = FetchAndCacheMetadataFromNetworkAsync();
                    return _cachedMetadata ?? new List<ContentMetadata>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading metadata from local storage. Falling back to network fetch.");
            }
            return await FetchAndCacheMetadataFromNetworkAsync();
        }

        private async Task<List<ContentMetadata>> FetchAndCacheMetadataFromNetworkAsync()
        {
            try
            {
                var path = $"{GITHUB_CONTENT_BASE_URL}metadata.json";
                _logger.Information("Fetching metadata from network: {Path}", path);
                var response = await _httpClient.GetAsync(path);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                _cachedMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _localStorage.SetItemAsync("content_metadata", json);
                _logger.Information("Successfully fetched and cached {Count} content metadata items from network.", _cachedMetadata?.Count ?? 0);
                return _cachedMetadata ?? new List<ContentMetadata>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching metadata from network.");
#if DEBUG
                _logger.Warning("Falling back to dummy content metadata for DEBUG build.");
                _cachedMetadata = Enumerable.Range(1, 10).Select(i => ContentMetadata.CreateDummy(i)).ToList();
#else
                throw;
#endif
                return _cachedMetadata ?? new List<ContentMetadata>();
            }
        }

        /// <inheritdoc />
        public async Task<string> GetMarkdownContentAsync(string contentPath)
        {
            var cacheKey = $"markdown_{contentPath}";
            string? markdownContent = null;
            try
            {
                markdownContent = await _localStorage.GetItemAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(markdownContent))
                {
                    _logger.Information("Loaded markdown from local storage for {ContentPath}", contentPath);
                    _ = FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey);
                    return markdownContent;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading markdown from local storage for {ContentPath}. Falling back to network fetch.", contentPath);
            }
            return await FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey);
        }

        /// <inheritdoc />
        public Task<string> RenderMarkdownToHtmlAsync(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseAutoIdentifiers()
                .Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            _logger.Information("Markdown converted to HTML.");
            return Task.FromResult(html);
        }

        /// <inheritdoc />
        public async Task<TData?> GetAnyJson<TData>(string path) where TData : class
        {
            var logger = Log.Logger.ForContext<ContentMarkdownService>();
            try
            {
                logger.Information("Loading JSON from: {Path}", path);
                var response = await _httpClient.GetAsync(path);
                response.EnsureSuccessStatusCode();
                logger.Information("Successful status code received from: {Path}", path);
                TData? result = await JsonSerializer.DeserializeAsync<TData>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                logger.Information("JSON loaded: {ResultType}", typeof(TData).Name);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading JSON from: {Path}", path);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetCategories()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Categories).Distinct().ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetTags()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Tags).Distinct().ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetKeywords()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Keywords ?? new List<string>()).Distinct().ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ContentMetadata>> GetContentByCategory(string category)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ContentMetadata>> GetContentByTagAsync(string tag)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ContentMetadata>> GetRelatedContent(ContentMetadata currentItem)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata
                .Where(post => post.Slug != currentItem.Slug && post.Tags.Any(tag => currentItem.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .OrderByDescending(post => post.PublishDate)
                .Take(5);
        }

        private async Task<string> FetchAndCacheMarkdownFromNetworkAsync(string contentPath, string cacheKey)
        {
            try
            {
                var url = $"{GITHUB_CONTENT_BASE_URL}{contentPath}";
                _logger.Information("Fetching markdown from network: {Url}", url);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var markdown = await response.Content.ReadAsStringAsync();
                await _localStorage.SetItemAsync(cacheKey, markdown);
                _logger.Information("Successfully fetched and cached markdown for {ContentPath}.", contentPath);
                return markdown;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch markdown from network for {ContentPath}.", contentPath);
                return $"<p>Error loading content: {ex.Message}</p>";
            }
        }
    }
}
