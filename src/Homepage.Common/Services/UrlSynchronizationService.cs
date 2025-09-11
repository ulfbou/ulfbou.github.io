using Homepage.Common.Models;

using Microsoft.AspNetCore.Components;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; // For HttpUtility.ParseQueryString and HttpUtility.UrlEncode/Decode

namespace Homepage.Common.Services
{
    /// <summary>
    /// Implements <see cref="IUrlSynchronizationService"/> for synchronizing application state with the URL.
    /// </summary>
    public class UrlSynchronizationService : IUrlSynchronizationService
    {
        private readonly NavigationManager _navigationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSynchronizationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The NavigationManager instance for URL interactions.</param>
        public UrlSynchronizationService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        /// <summary>
        /// Parses search text and active tags from the current URL's query string.
        /// </summary>
        /// <returns>A tuple containing SearchText, ActiveTags, and Audience, or null if no relevant parameters are found.</returns>
        public (string SearchText, IEnumerable<string> ActiveTags, string Audience)? ParseFiltersFromUrl()
        {
            var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);

            string searchText = query["search"] ?? string.Empty;
            string audience = query["audience"] ?? string.Empty;
            List<string> activeTags = new List<string>();

            if (query["tags"] != null)
            {
                activeTags = query["tags"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(t => HttpUtility.UrlDecode(t).Trim())
                                           .ToList()
                    ?? new List<string>();
            }

            if (!string.IsNullOrEmpty(searchText) || activeTags.Any() || !string.IsNullOrEmpty(audience))
            {
                Log.ForContext("Class", nameof(UrlSynchronizationService))
                   .ForContext("Method", nameof(ParseFiltersFromUrl))
                   .Debug("Parsed filters from URL: Search='{Search}', Tags='{Tags}', Audience='{Audience}'", searchText, string.Join(",", activeTags), audience);
                return (searchText, activeTags, audience);
            }

            return null;
        }

        /// <summary>
        /// Synchronizes the current filter state (search text, active tags, audience) to the URL's query string.
        /// </summary>
        /// <param name="searchText">The current search text.</param>
        /// <param name="activeTags">The currently active tag filters.</param>
        /// <param name="audience">The current audience.</param>
        public void SynchronizeFiltersToUrl(string searchText, IEnumerable<string> activeTags, string audience)
        {
            var uriBuilder = new UriBuilder(_navigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query.Remove("search");
            query.Remove("tags");
            query.Remove("audience");

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query["search"] = HttpUtility.UrlEncode(searchText);
            }

            if (activeTags != null && activeTags.Any())
            {
                query["tags"] = HttpUtility.UrlEncode(string.Join(",", activeTags.Select(t => t.Trim())));
            }

            if (!string.IsNullOrWhiteSpace(audience))
            {
                query["audience"] = HttpUtility.UrlEncode(audience);
            }

            uriBuilder.Query = query.ToString();
            var newUri = uriBuilder.Uri.ToString();

            if (_navigationManager.Uri != newUri)
            {
                _navigationManager.NavigateTo(newUri, replace: true);
                Log.ForContext("Class", nameof(UrlSynchronizationService))
                   .ForContext("Method", nameof(SynchronizeFiltersToUrl))
                   .Debug("URL synchronized to: {NewUri}", newUri);
            }
        }

        /// <summary>
        /// Parses the content slug from the current URL's path.
        /// Assumes content slugs are in the format `/content/{slug}`.
        /// </summary>
        /// <returns>The content slug, or null if not found.</returns>
        public string? ParseContentSlugFromUrl()
        {
            var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            var pathSegments = uri.Segments;

            if (pathSegments.Length >= 2 && pathSegments[pathSegments.Length - 2].Trim('/').Equals("content", StringComparison.OrdinalIgnoreCase))
            {
                var slug = pathSegments.Last().Trim('/');
                Log.ForContext("Class", nameof(UrlSynchronizationService))
                   .ForContext("Method", nameof(ParseContentSlugFromUrl))
                   .Debug("Parsed content slug from URL: {Slug}", slug);
                return slug;
            }
            return null;
        }

        /// <summary>
        /// Synchronizes the content slug to the URL's path.
        /// </summary>
        /// <param name="slug">The content slug to navigate to, or null to navigate to the base path.</param>
        public void SynchronizeContentToUrl(string? slug)
        {
            string newPath = string.IsNullOrEmpty(slug) ? "/" : $"/content/{slug}";
            var currentUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

            var uriBuilder = new UriBuilder(currentUri);
            uriBuilder.Path = newPath;
            var finalUri = uriBuilder.Uri.ToString();

            if (_navigationManager.Uri != finalUri)
            {
                _navigationManager.NavigateTo(finalUri, replace: true);
                Log.ForContext("Class", nameof(UrlSynchronizationService))
                   .ForContext("Method", nameof(SynchronizeContentToUrl))
                   .Debug("Content URL synchronized to: {NewUri}", finalUri);
            }
        }
    }
}
