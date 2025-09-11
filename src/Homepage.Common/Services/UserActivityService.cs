using Homepage.Common.Constants;
using Homepage.Common.Models;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Manages and provides insights into user activity and preferences, utilizing local storage.
    /// </summary>
    public class UserActivityService : IUserActivityService
    {
        private readonly ILocalStorageService _localStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserActivityService"/> class.
        /// </summary>
        /// <param name="localStorageService">The local storage service dependency.</param>
        public UserActivityService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        /// <summary>Records a content view event for a given slug.</summary>
        /// <param name="slug">The slug of the content that was viewed.</param>
        public async Task RecordContentViewAsync(string slug)
        {
            try
            {
                await _localStorageService.AddViewedSlugAsync(slug);
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(RecordContentViewAsync))
                   .Debug("Content view recorded for slug: {Slug}", slug);
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(RecordContentViewAsync))
                   .Error(ex, "Failed to record content view for slug: {Slug}", slug);
            }
        }

        /// <summary>Gets a limited list of recently viewed content slugs.</summary>
        /// <param name="count">The maximum number of slugs to retrieve.</param>
        /// <returns>A list of recently viewed slugs.</returns>
        public async Task<List<string>> GetRecentlyViewedSlugsAsync(int count)
        {
            try
            {
                var slugs = await _localStorageService.GetViewedSlugsAsync();
                return slugs.Take(count).ToList();
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(GetRecentlyViewedSlugsAsync))
                   .Error(ex, "Failed to get recently viewed slugs.");
                return new List<string>();
            }
        }

        /// <summary>Gets all viewed content slugs.</summary>
        /// <returns>A list of all viewed slugs.</returns>
        public async Task<List<string>> GetAllViewedSlugsAsync()
        {
            try
            {
                return await _localStorageService.GetViewedSlugsAsync();
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(GetAllViewedSlugsAsync))
                   .Error(ex, "Failed to get all viewed slugs.");
                return new List<string>();
            }
        }

        /// <summary>Sets the pinned status for a content slug.</summary>
        /// <param name="slug">The slug of the content to pin or unpin.</param>
        /// <param name="isPinned">True to pin, false to unpin.</param>
        public async Task SetPinnedSlugAsync(string slug, bool isPinned)
        {
            try
            {
                var pinnedSlugs = await _localStorageService.GetPinnedSlugsAsync();
                if (isPinned)
                {
                    pinnedSlugs.Add(slug);
                    Log.ForContext("Class", nameof(UserActivityService))
                       .ForContext("Method", nameof(SetPinnedSlugAsync))
                       .Debug("Pinned slug: {Slug}", slug);
                }
                else
                {
                    pinnedSlugs.Remove(slug);
                    Log.ForContext("Class", nameof(UserActivityService))
                       .ForContext("Method", nameof(SetPinnedSlugAsync))
                       .Debug("Unpinned slug: {Slug}", slug);
                }
                await _localStorageService.SetPinnedSlugsAsync(pinnedSlugs);
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(SetPinnedSlugAsync))
                   .Error(ex, "Failed to set pinned status for slug: {Slug}", slug);
            }
        }

        /// <summary>Gets a hash set of all currently pinned content slugs.</summary>
        /// <returns>A hash set of pinned slugs.</returns>
        public async Task<HashSet<string>> GetPinnedSlugsAsync()
        {
            try
            {
                return await _localStorageService.GetPinnedSlugsAsync();
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(GetPinnedSlugsAsync))
                   .Error(ex, "Failed to get pinned slugs.");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>Retrieves the timestamp of the user's last visit.</summary>
        /// <returns>The <see cref="DateTimeOffset"/> of the last visit, or null if not recorded.</returns>
        public async Task<DateTimeOffset?> GetLastVisitTimestampAsync()
        {
            try
            {
                return await _localStorageService.GetLastVisitTimestampAsync();
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(GetLastVisitTimestampAsync))
                   .Error(ex, "Failed to get last visit timestamp.");
                return null;
            }
        }

        /// <summary>Updates the last visit timestamp to the current UTC time.</summary>
        public async Task UpdateLastVisitTimestampAsync()
        {
            try
            {
                await _localStorageService.SetLastVisitTimestampAsync(DateTimeOffset.UtcNow);
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(UpdateLastVisitTimestampAsync))
                   .Debug("Last visit timestamp updated.");
            }
            catch (Exception ex)
            {
                Log.ForContext("Class", nameof(UserActivityService))
                   .ForContext("Method", nameof(UpdateLastVisitTimestampAsync))
                   .Error(ex, "Failed to update last visit timestamp.");
            }
        }
    }
}