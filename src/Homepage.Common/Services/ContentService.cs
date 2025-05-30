// --- Homepage.Common.Services/ContentService.cs (Corrected) ---
using System.Text.Json;
using Serilog;
using Homepage.Common.Models;
using Markdig; // Required for Markdown.ToHtml
using Blazored.LocalStorage; // Required for caching

namespace Homepage.Common.Services
{
    public class ContentService // No longer takes HttpClient in constructor params here, as it's injected directly
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage; // Inject ILocalStorageService
        private List<ContentMetadata>? _allContentMetadata;

        // IMPORTANT: Replace with your actual GitHub Pages content URL
        // Example: If your content repo is at https://github.com/yourusername/portfolio-content
        // and your files are in the root or a 'docs' folder, adjust accordingly.
        // For this example, assuming content is directly under 'portfolio-content' repo.
#if DEBUG
        // Local development URL for testing purposes
        public const string GITHUB_CONTENT_BASE_URL = "Posts/"; // Local path for development 
#else
        public const string GITHUB_CONTENT_BASE_URL = "https://ulfbou.github.io/Posts/"; 
#endif

        public string BaseUri { get; set; } = GITHUB_CONTENT_BASE_URL;

        public ContentService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<List<ContentMetadata>> GetContentMetadataAsync()
        {
            var logger = Log.Logger.ForContext<ContentService>();

            if (_allContentMetadata != null)
            {
                _ = FetchAndCacheMetadataFromNetworkAsync();
                return _allContentMetadata;
            }

            try
            {
                // Attempt to load from local storage first
                var cachedJson = await _localStorage.GetItemAsync<string>("content_metadata");
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    _allContentMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(cachedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    logger.Information("Loaded metadata from local storage.");
                    _ = FetchAndCacheMetadataFromNetworkAsync(); // Fire and forget to revalidate in background
                    return _allContentMetadata ?? new List<ContentMetadata>();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading metadata from local storage. Falling back to network fetch.");
                // Fall through to network fetch if cache fails
            }

            return await FetchAndCacheMetadataFromNetworkAsync();
        }

        private async Task<List<ContentMetadata>> FetchAndCacheMetadataFromNetworkAsync()
        {
            var logger = Log.Logger.ForContext<ContentService>();
            try
            {
                var path = $"{GITHUB_CONTENT_BASE_URL}metadata.json";
                logger.Information("Fetching metadata from network: {Path}", path);
                var response = await _httpClient.GetAsync(path);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                _allContentMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_allContentMetadata == null)
                {
                    logger.Warning("Metadata deserialized to null from network. Initializing empty list.");
                    _allContentMetadata = new List<ContentMetadata>();
                }

                // Cache the fresh data
                await _localStorage.SetItemAsync("content_metadata", json);
                logger.Information("Successfully fetched and cached {Count} content metadata items from network.", _allContentMetadata.Count);
                return _allContentMetadata;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error fetching metadata from network.");
#if DEBUG
                logger.Warning("Falling back to dummy content metadata for DEBUG build.");
                _allContentMetadata = Enumerable.Range(1, 10).Select(i => ContentMetadata.CreateDummy(i)).ToList();
#else
                throw; // Re-throw in production for critical failure
#endif
                return _allContentMetadata ?? new List<ContentMetadata>(); // Return dummy or empty list
            }
        }

        // Renamed and updated GetAnyJson for clarity, returning TData?
        public async Task<TData?> GetAnyJson<TData>(string path) where TData : class
        {
            var logger = Log.Logger.ForContext<ContentService>();
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

        // --- NEW/CORRECTED: GetMarkdownContentAsync ---
        public async Task<string> GetMarkdownContentAsync(string contentPath)
        {
            var logger = Log.Logger.ForContext<ContentService>();
            var cacheKey = $"markdown_{contentPath}";
            string? markdownContent = null;

            try
            {
                // Try to get from local storage first
                markdownContent = await _localStorage.GetItemAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(markdownContent))
                {
                    logger.Information("Loaded markdown from local storage for {ContentPath}", contentPath);
                    // Return cached content immediately, then revalidate in background
                    _ = FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey); // Fire and forget
                    return markdownContent;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading markdown from local storage for {ContentPath}. Falling back to network fetch.", contentPath);
                // Fall through to network fetch
            }

            // If not in cache or error, fetch from network
            return await FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey);
        }

        private async Task<string> FetchAndCacheMarkdownFromNetworkAsync(string contentPath, string cacheKey)
        {
            var logger = Log.Logger.ForContext<ContentService>();
            try
            {
                var url = $"{GITHUB_CONTENT_BASE_URL}{contentPath}";
                logger.Information("Fetching markdown from network: {Url}", url);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var markdown = await response.Content.ReadAsStringAsync();

                // Cache the fresh data
                await _localStorage.SetItemAsync(cacheKey, markdown);
                logger.Information("Successfully fetched and cached markdown for {ContentPath}.", contentPath);
                return markdown;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to fetch markdown from network for {ContentPath}.", contentPath);
                return $"<p>Error loading content: {ex.Message}</p>"; // Return an error message as HTML
            }
        }

        // --- NEW/CORRECTED: RenderMarkdownToHtmlAsync ---
        public Task<string> RenderMarkdownToHtmlAsync(string markdown)
        {
            var logger = Log.Logger.ForContext<ContentService>();
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions() // Enable tables, task lists, etc.
                .UseYamlFrontMatter() // Crucial for parsing YAML front matter
                .Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            logger.Information("Markdown converted to HTML.");
            return Task.FromResult(html);
        }

        // Methods to derive categories, tags, keywords from _allContentMetadata
        // These will now call GetContentMetadataAsync() first if data isn't loaded.
        public async Task<IEnumerable<string>> GetCategories()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Categories).Distinct().ToList();
        }

        public async Task<IEnumerable<string>> GetTags()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Tags).Distinct().ToList();
        }

        public async Task<IEnumerable<string>> GetKeywords()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Keywords ?? new List<string>()).Distinct().ToList();
        }

        public async Task<IEnumerable<ContentMetadata>> GetContentByCategory(string category)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<ContentMetadata>> GetContentByTagAsync(string tag)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<ContentMetadata>> GetRelatedContent(ContentMetadata currentItem)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata
                .Where(post => post.Slug != currentItem.Slug && post.Tags.Any(tag => currentItem.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .OrderByDescending(post => post.PublishDate)
                .Take(5);
        }
    }
}
