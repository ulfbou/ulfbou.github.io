using Homepage.Common.Models;

namespace Homepage.Common.Services
{
    public interface ILocalStorageService
    {
        /// <summary>Retrieves a list of viewed content slugs from local storage.</summary>
        /// <returns>A list of viewed slugs.</returns>
        Task<List<string>> GetViewedSlugsAsync();

        /// <summary>
        /// Adds a slug to the list of viewed content in local storage.
        /// Limits the number of stored slugs to <see cref="AppConstants.MaxRecentlyViewedItems"/>.
        /// </summary>
        /// <param name="slug">The slug to add.</param>
        Task AddViewedSlugAsync(string slug);

        /// <summary>Retrieves a hash set of pinned content slugs from local storage.</summary>
        /// <returns>A hash set of pinned slugs.</returns>
        Task<HashSet<string>> GetPinnedSlugsAsync();

        /// <summary>Sets the hash set of pinned content slugs in local storage.</summary>
        /// <param name="pinnedSlugs">The hash set of slugs to store.</param>
        Task SetPinnedSlugsAsync(HashSet<string> pinnedSlugs);

        /// <summary>Retrieves the last visit timestamp from local storage.</summary>
        /// <returns>The last visit timestamp, or null if not found.</returns>
        Task<DateTimeOffset?> GetLastVisitTimestampAsync();

        /// <summary>Sets the last visit timestamp in local storage.</summary>
        /// <param name="timestamp">The timestamp to store.</param>
        Task SetLastVisitTimestampAsync(DateTimeOffset timestamp);

        /// <summary>Sets the cached content metadata in local storage.</summary>
        /// <param name="cacheData">The <see cref="ContentMetadataCache"/> object to store.</param>
        Task SetContentMetadataCacheAsync(ContentMetadataCache cacheData);

        /// <summary>Retrieves the cached content metadata from local storage.</summary>
        /// <returns>The cached <see cref="ContentMetadataCache"/> object, or null if not found.</returns>
        Task<ContentMetadataCache?> GetContentMetadataCacheAsync();
    }
}
