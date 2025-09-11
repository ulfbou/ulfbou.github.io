using Homepage.Common.Constants;
using Homepage.Common.Models;
using Markdig.Extensions.AutoIdentifiers;
using Markdig;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http.Json;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Service for loading and rendering markdown content, including caching of metadata.
    /// </summary>
    public class ContentMarkdownService : IMarkdownService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;
        private readonly MarkdownPipeline _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentMarkdownService"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance for making HTTP requests.</param>
        /// <param name="localStorageService">The local storage service for caching.</param>
        public ContentMarkdownService(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseAutoIdentifiers(AutoIdentifierOptions.AutoLink)
                .UseYamlFrontMatter()
                .Build();
        }

        /// <summary>
        /// Retrieves all content metadata, prioritizing cached data if fresh, otherwise fetching from network.
        /// </summary>
        /// <returns>A list of <see cref="ContentMetadata"/> items.</returns>
        public async Task<List<ContentMetadata>> GetContentMetadataAsync()
        {
            var logger = Log.ForContext("Class", nameof(ContentMarkdownService))
                           .ForContext("Method", nameof(GetContentMetadataAsync));

            try
            {
                var cachedData = await _localStorageService.GetContentMetadataCacheAsync();
                if (cachedData != null && (DateTimeOffset.UtcNow - cachedData.LastModified).TotalHours < 1)
                {
                    logger.Information("Loaded metadata from local storage.");
                    _ = FetchAndCacheMetadataFromNetworkAsync();
                    return cachedData.Metadata;
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to load metadata from local storage cache. Proceeding to network fetch.");
            }

            return await FetchAndCacheMetadataFromNetworkAsync();
        }

        /// <summary>
        /// Fetches content metadata from the network and caches it to local storage.
        /// </summary>
        /// <returns>A list of <see cref="ContentMetadata"/> items.</returns>
        private async Task<List<ContentMetadata>> FetchAndCacheMetadataFromNetworkAsync()
        {
            var logger = Log.ForContext("Class", nameof(ContentMarkdownService))
                           .ForContext("Method", nameof(FetchAndCacheMetadataFromNetworkAsync));

            logger.Information("Fetching metadata from network: {MetadataPath}", AppConstants.ContentMetadataRelativePath);
            try
            {
                var response = await _httpClient.GetAsync(AppConstants.ContentMetadataRelativePath);
                response.EnsureSuccessStatusCode();

                var metadata = await response.Content.ReadFromJsonAsync<List<ContentMetadata>>();
                if (metadata == null)
                {
                    logger.Warning("Fetched metadata was null or empty.");
                    return new List<ContentMetadata>();
                }

                var newCache = new ContentMetadataCache
                {
                    Metadata = metadata,
                    LastModified = DateTimeOffset.UtcNow
                };
                await _localStorageService.SetContentMetadataCacheAsync(newCache);
                logger.Information("Successfully fetched and cached {Count} content metadata items from network.", metadata.Count);

                return metadata;
            }
            catch (HttpRequestException httpEx)
            {
                logger.Error(httpEx, "HTTP request failed to fetch metadata from {MetadataPath}. Status: {StatusCode}", AppConstants.ContentMetadataRelativePath, httpEx.StatusCode);
                return new List<ContentMetadata>();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An unexpected error occurred while fetching content metadata from {MetadataPath}.", AppConstants.ContentMetadataRelativePath);
                return new List<ContentMetadata>();
            }
        }

        /// <summary>
        /// Retrieves markdown content from the specified path.
        /// Note: This method does NOT cache individual markdown files due to the current ILocalStorageService interface.
        /// </summary>
        /// <param name="contentPath">The relative path to the markdown file.</param>
        /// <returns>The markdown content as a string.</returns>
        public async Task<string> GetMarkdownContentAsync(string contentPath)
        {
            var logger = Log.ForContext("Class", nameof(ContentMarkdownService))
                           .ForContext("Method", nameof(GetMarkdownContentAsync));

            // Prepend GITHUB_CONTENT_BASE_URL if content is hosted externally
            string fullUrl = $"{AppConstants.GithubContentBaseUrl}{contentPath}";

            logger.Information("Fetching markdown content from: {Url}", fullUrl);
            try
            {
                var response = await _httpClient.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();
                var markdown = await response.Content.ReadAsStringAsync();
                return markdown;
            }
            catch (HttpRequestException httpEx)
            {
                logger.Error(httpEx, "HTTP request failed to fetch markdown from {ContentPath}. Status: {StatusCode}", fullUrl, httpEx.StatusCode);
                throw new ApplicationException($"Failed to load markdown content from {contentPath}. Please check the path and network connection.", httpEx);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An unexpected error occurred while fetching markdown content from {ContentPath}.", fullUrl);
                throw new ApplicationException($"An error occurred while loading content from {contentPath}.", ex);
            }
        }

        /// <summary>
        /// Renders markdown content to HTML using Markdig.
        /// </summary>
        /// <param name="markdown">The markdown string to render.</param>
        /// <returns>The rendered HTML string.</returns>
        public Task<string> RenderMarkdownToHtmlAsync(string markdown)
        {
            var html = Markdown.ToHtml(markdown, _pipeline);
            Log.ForContext("Class", nameof(ContentMarkdownService))
               .ForContext("Method", nameof(RenderMarkdownToHtmlAsync))
               .Information("Markdown converted to HTML.");
            return Task.FromResult(html);
        }

        /// <summary>
        /// Renders markdown content to HTML and generates a Table of Contents (TOC) HTML.
        /// </summary>
        /// <param name="markdown">The markdown string to render and generate TOC from.</param>
        /// <returns>A tuple containing the rendered HTML and the TOC HTML.</returns>
        public Task<(string html, string tocHtml)> RenderMarkdownWithTocAsync(string markdown)
        {
            var html = Markdown.ToHtml(markdown, _pipeline);
            var tocHtml = GenerateTocHtml(markdown);
            Log.ForContext("Class", nameof(ContentMarkdownService))
               .ForContext("Method", nameof(RenderMarkdownWithTocAsync))
               .Information("Generated TOC HTML.");
            return Task.FromResult((html, tocHtml));
        }

        /// <summary>
        /// Generates HTML for a Table of Contents from markdown headings.
        /// </summary>
        /// <param name="markdownContent">The markdown content string.</param>
        /// <returns>The HTML string representing the Table of Contents.</returns>
        private string GenerateTocHtml(string markdownContent)
        {
            var toc = new StringBuilder();
            toc.AppendLine("<ul>");
            var headingRegex = new Regex(@"^(\#{1,6})\s*(.*?)$", RegexOptions.Multiline);
            foreach (Match match in headingRegex.Matches(markdownContent))
            {
                var level = match.Groups[1].Length;
                var headingText = match.Groups[2].Value.Trim();
                if (headingText.Contains("[[TOC]]", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var slug = headingText.ToLowerInvariant();
                slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
                slug = Regex.Replace(slug, @"\s+", "-");
                slug = slug.Trim('-');

                var indent = new string(' ', (level - 1) * 2);
                toc.AppendLine($"{indent}<li class=\"toc-level-{level}\"><a href=\"#{slug}\">{headingText}</a></li>");
            }
            toc.AppendLine("</ul>");
            return toc.ToString();
        }

        /// <summary>
        /// Generic method to load and deserialize JSON data from a given path.
        /// </summary>
        /// <typeparam name="TData">The type to deserialize the JSON into.</typeparam>
        /// <param name="path">The relative path to the JSON file.</param>
        /// <returns>The deserialized data, or null if an error occurs.</returns>
        public async Task<TData?> GetAnyJson<TData>(string path) where TData : class
        {
            var logger = Log.ForContext("Class", nameof(ContentMarkdownService))
                           .ForContext("Method", nameof(GetAnyJson));
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

        /// <summary>Gets a list of all distinct categories from the content metadata.</summary>
        /// <returns>An enumerable of category strings.</returns>
        public async Task<IEnumerable<string>> GetCategories()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Categories).Distinct().ToList();
        }

        /// <summary>Gets a list of all distinct tags from the content metadata.</summary>
        /// <returns>An enumerable of tag strings.</returns>
        public async Task<IEnumerable<string>> GetTags()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Tags).Distinct().ToList();
        }

        /// <summary>Gets a list of all distinct keywords from the content metadata.</summary>
        /// <returns>An enumerable of keyword strings.</returns>
        public async Task<IEnumerable<string>> GetKeywords()
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.SelectMany(item => item.Keywords ?? new List<string>()).Distinct().ToList();
        }

        /// <summary>Gets content metadata filtered by a specific category.</summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>An enumerable of <see cref="ContentMetadata"/> items.</returns>
        public async Task<IEnumerable<ContentMetadata>> GetContentByCategory(string category)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>Gets content metadata filtered by a specific tag.</summary>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>An enumerable of <see cref="ContentMetadata"/> items.</returns>
        public async Task<IEnumerable<ContentMetadata>> GetContentByTagAsync(string tag)
        {
            var metadata = await GetContentMetadataAsync();
            return metadata.Where(post => post.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>Gets related content based on shared tags with a given content item.</summary>
        /// <param name="currentItem">The current content item to find related content for.</param>
        /// <returns>An enumerable of related <see cref="ContentMetadata"/> items.</returns>
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
