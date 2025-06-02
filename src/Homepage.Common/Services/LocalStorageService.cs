using Microsoft.JSInterop;

using Serilog;

using System.Text.Json;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Service for interacting with browser's local storage to track user preferences and viewed content.
    /// </summary>
    public class LocalStorageService
    {
        private readonly IJSRuntime _js;
        private readonly ILogger _logger;
        private const string ViewedSlugsKey = "viewedSlugs";
        private const string PinnedProjectsKey = "pinnedProjects";

        public LocalStorageService(IJSRuntime js)
        {
            _js = js;
            _logger = Log.Logger.ForContext<LocalStorageService>();
        }

        /// <summary>
        /// Retrieves a list of slugs for content items that have been viewed by the user.
        /// </summary>
        /// <returns>A list of viewed content slugs.</returns>
        public async Task<List<string>> GetViewedSlugsAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorageHelper.getItem", ViewedSlugsKey);
                return string.IsNullOrWhiteSpace(json) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(json)?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve viewed slugs from local storage.");
                return new List<string>();
            }
        }

        /// <summary>
        /// Adds a new slug to the list of viewed content items and persists it to local storage.
        /// </summary>
        /// <param name="slug">The slug of the content item to add.</param>
        public async Task AddViewedSlugAsync(string slug)
        {
            try
            {
                var viewed = await GetViewedSlugsAsync();
                if (!viewed.Contains(slug, StringComparer.OrdinalIgnoreCase))
                {
                    viewed.Add(slug);
                    var json = JsonSerializer.Serialize(viewed.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
                    await _js.InvokeVoidAsync("localStorageHelper.setItem", ViewedSlugsKey, json);
                    _logger.Information("Added slug '{Slug}' to viewed items.", slug);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add slug '{Slug}' to viewed items in local storage.", slug);
            }
        }

        /// <summary>
        /// Retrieves the set of titles for projects that have been pinned by the user.
        /// </summary>
        /// <returns>A HashSet of pinned project titles.</returns>
        public async Task<HashSet<string>> GetPinnedProjectTitlesAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorageHelper.getItem", PinnedProjectsKey);
                return string.IsNullOrWhiteSpace(json) ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : JsonSerializer.Deserialize<HashSet<string>>(json)?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve pinned project titles from local storage.");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Persists the current set of pinned project titles to local storage.
        /// </summary>
        /// <param name="pinnedTitles">The HashSet of project titles to save.</param>
        public async Task SetPinnedProjectTitlesAsync(HashSet<string> pinnedTitles)
        {
            try
            {
                var json = JsonSerializer.Serialize(pinnedTitles);
                await _js.InvokeVoidAsync("localStorageHelper.setItem", PinnedProjectsKey, json);
                _logger.Information("Updated pinned projects in local storage. Count: {Count}", pinnedTitles.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save pinned project titles to local storage.");
            }
        }
    }
}
