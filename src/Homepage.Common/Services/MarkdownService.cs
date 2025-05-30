using System.Text.Json;

using Blazored.LocalStorage;

using Homepage.Common.Models;

using Markdig;

namespace Homepage.Common.Services
{
    /// <summary>A service for managing Markdown content.</summary>
    public class MarkdownService : IMarkdownService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService? _localStorage;
        private List<ContentMetadata>? _cachedMetadata;

        private const string GITHUB_CONTENT_BASE_URL = ContentService.GITHUB_CONTENT_BASE_URL;

        /// <summary>Initializes a new instance of the <see cref="MarkdownService"/> class.</summary>
        /// <param name="httpClient">The HTTP client used to fetch content.</param>
        /// <param name="localStorage">The local storage service used for caching.</param>
        public MarkdownService(HttpClient httpClient, ILocalStorageService? localStorage = null)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        /// <inheritdoc />
        public async Task<List<ContentMetadata>> GetContentMetadataAsync()
        {
            if (_cachedMetadata != null)
            {
                return _cachedMetadata;
            }

            try
            {
                string? cachedJson = null;

                if (_localStorage is not null)
                {
                    cachedJson = await _localStorage.GetItemAsync<string>("content_metadata");
                }

                if (!string.IsNullOrEmpty(cachedJson))
                {
                    _cachedMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(cachedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _ = FetchAndCacheMetadataFromNetworkAsync();
                    return _cachedMetadata ?? new List<ContentMetadata>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading metadata from local storage: {ex.Message}");
            }

            return await FetchAndCacheMetadataFromNetworkAsync();
        }

        private async Task<List<ContentMetadata>> FetchAndCacheMetadataFromNetworkAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GITHUB_CONTENT_BASE_URL}metadata.json");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                _cachedMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_localStorage is not null)
                {
                    await _localStorage.SetItemAsync("content_metadata", json);
                }

                return _cachedMetadata ?? new List<ContentMetadata>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching metadata from network: {ex.Message}");
                return new List<ContentMetadata>();
            }
        }

        public async Task<string> GetMarkdownContentAsync(string contentPath)
        {
            var cacheKey = $"markdown_{contentPath}";
            string? markdownContent = null;

            try
            {
                if (_localStorage is not null)
                {
                    markdownContent = await _localStorage.GetItemAsync<string>(cacheKey);
                }

                if (!string.IsNullOrEmpty(markdownContent))
                {
                    _ = FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey);
                    return markdownContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading markdown from local storage for {contentPath}: {ex.Message}");
            }

            return await FetchAndCacheMarkdownFromNetworkAsync(contentPath, cacheKey);
        }

        private async Task<string> FetchAndCacheMarkdownFromNetworkAsync(string contentPath, string cacheKey)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GITHUB_CONTENT_BASE_URL}{contentPath}");
                response.EnsureSuccessStatusCode();
                var markdown = await response.Content.ReadAsStringAsync();

                // Cache the fresh data
                if (_localStorage is not null)
                {
                    await _localStorage.SetItemAsync(cacheKey, markdown);
                }

                return markdown;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching markdown from network for {contentPath}: {ex.Message}");
                return $"<p>Error loading content: {ex.Message}</p>";
            }
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
            return Task.FromResult(html);
        }
    }
}
