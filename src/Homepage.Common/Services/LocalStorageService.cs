using Homepage.Common.Constants;
using Homepage.Common.Models;

using Microsoft.JSInterop;

using Serilog;

using System.Text.Json;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Implements <see cref="ILocalStorageService"/> for interacting with browser's local storage.
    /// </summary>
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
        /// </summary>
        /// <param name="jsRuntime">The JSRuntime instance for JavaScript interop.</param>
        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Retrieves a list of viewed content slugs from local storage.
        /// </summary>
        /// <returns>A list of viewed slugs.</returns>
        public async Task<List<string>> GetViewedSlugsAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>(AppConstants.JsLocalStorageHelperGetItem, AppConstants.LocalStorageViewedSlugsKey);
                if (string.IsNullOrEmpty(json))
                {
                    return new List<string>();
                }
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(GetViewedSlugsAsync))
                   .Error(ex, "Failed to retrieve viewed slugs from local storage.");
                return new List<string>();
            }
        }

        /// <summary>
        /// Adds a slug to the list of viewed content in local storage.
        /// Limits the number of stored slugs to <see cref="AppConstants.MaxRecentlyViewedItems"/>.
        /// </summary>
        /// <param name="slug">The slug to add.</param>
        public async Task AddViewedSlugAsync(string slug)
        {
            try
            {
                var slugs = await GetViewedSlugsAsync();
                slugs.RemoveAll(s => s.Equals(slug, StringComparison.OrdinalIgnoreCase)); // Remove if already exists to move to front
                slugs.Insert(0, slug); // Add to the beginning

                if (slugs.Count > AppConstants.MaxRecentlyViewedItems)
                {
                    slugs = slugs.Take(AppConstants.MaxRecentlyViewedItems).ToList();
                }
                await _jsRuntime.InvokeVoidAsync(AppConstants.JsLocalStorageHelperSetItem, AppConstants.LocalStorageViewedSlugsKey, JsonSerializer.Serialize(slugs));
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(AddViewedSlugAsync))
                   .Error(ex, "Failed to add viewed slug to local storage.");
            }
        }

        /// <summary>
        /// Retrieves a hash set of pinned content slugs from local storage.
        /// </summary>
        /// <returns>A hash set of pinned slugs.</returns>
        public async Task<HashSet<string>> GetPinnedSlugsAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>(AppConstants.JsLocalStorageHelperGetItem, AppConstants.LocalStoragePinnedSlugsKey);
                if (string.IsNullOrEmpty(json))
                {
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(GetPinnedSlugsAsync))
                   .Error(ex, "Failed to retrieve pinned slugs from local storage.");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Sets the hash set of pinned content slugs in local storage.
        /// </summary>
        /// <param name="pinnedSlugs">The hash set of slugs to store.</param>
        public async Task SetPinnedSlugsAsync(HashSet<string> pinnedSlugs)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(AppConstants.JsLocalStorageHelperSetItem, AppConstants.LocalStoragePinnedSlugsKey, JsonSerializer.Serialize(pinnedSlugs));
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(SetPinnedSlugsAsync))
                   .Error(ex, "Failed to set pinned slugs in local storage.");
            }
        }

        /// <summary>
        /// Retrieves the last visit timestamp from local storage.
        /// </summary>
        /// <returns>The last visit timestamp, or null if not found.</returns>
        public async Task<DateTimeOffset?> GetLastVisitTimestampAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>(AppConstants.JsLocalStorageHelperGetItem, AppConstants.LocalStorageLastVisitKey);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }
                return JsonSerializer.Deserialize<DateTimeOffset>(json);
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(GetLastVisitTimestampAsync))
                   .Error(ex, "Failed to retrieve last visit timestamp from local storage.");
                return null;
            }
        }

        /// <summary>
        /// Sets the last visit timestamp in local storage.
        /// </summary>
        /// <param name="timestamp">The timestamp to store.</param>
        public async Task SetLastVisitTimestampAsync(DateTimeOffset timestamp)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(AppConstants.JsLocalStorageHelperSetItem, AppConstants.LocalStorageLastVisitKey, JsonSerializer.Serialize(timestamp));
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(SetLastVisitTimestampAsync))
                   .Error(ex, "Failed to set last visit timestamp in local storage.");
            }
        }

        /// <summary>
        /// Sets the cached content metadata in local storage.
        /// </summary>
        /// <param name="cacheData">The <see cref="ContentMetadataCache"/> object to store.</param>
        public async Task SetContentMetadataCacheAsync(ContentMetadataCache cacheData)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(AppConstants.JsLocalStorageHelperSetItem, AppConstants.LocalStorageContentMetadataCacheKey, JsonSerializer.Serialize(cacheData));
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(SetContentMetadataCacheAsync))
                   .Error(ex, "Failed to set content metadata cache in local storage.");
            }
        }

        /// <summary>
        /// Retrieves the cached content metadata from local storage.
        /// </summary>
        /// <returns>The cached <see cref="ContentMetadataCache"/> object, or null if not found.</returns>
        public async Task<ContentMetadataCache?> GetContentMetadataCacheAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>(AppConstants.JsLocalStorageHelperGetItem, AppConstants.LocalStorageContentMetadataCacheKey);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }
                return JsonSerializer.Deserialize<ContentMetadataCache>(json);
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(LocalStorageService))
                   .ForContext("Method", nameof(GetContentMetadataCacheAsync))
                   .Error(ex, "Failed to retrieve content metadata cache from local storage.");
                return null;
            }
        }
    }
}
