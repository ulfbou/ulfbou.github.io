using Homepage.Common.Models;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Manages global filtering and searching of content metadata.
    /// Holds the raw content, current search text, active filters, and provides filtered results.
    /// </summary>
    public class FilterService
    {
        private List<ContentMetadata> _allContentMetadata = new List<ContentMetadata>();
        private string _currentSearchText = string.Empty;
        private HashSet<string> _activeTagFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private string _currentAudience = string.Empty;

        /// <summary>Event raised when any filter parameter (search, tags, audience) changes.</summary>
        public event Action? OnFilterParametersChanged;

        /// <summary>
        /// Gets or sets the current search text used for filtering content.
        /// The setter centralizes trimming and normalizing the search text to lower case for consistent filtering.
        /// </summary>
        public string CurrentSearchText
        {
            get => _currentSearchText;
            set
            {
                var searchText = value?.Trim().ToLowerInvariant() ?? string.Empty;
                if (_currentSearchText != searchText)
                {
                    _currentSearchText = searchText;
                    OnFilterParametersChanged?.Invoke();
                    Log.ForContext("Class", nameof(FilterService))
                       .ForContext("Method", nameof(CurrentSearchText))
                       .Debug("Search text set to: {SearchText}", _currentSearchText);
                }
            }
        }

        /// <summary>Initializes the service with all content metadata. Should be called once.</summary>
        /// <param name="allContent">The complete list of content metadata.</param>
        public void InitializeContent(List<ContentMetadata> allContent)
            => _allContentMetadata = allContent ?? new List<ContentMetadata>();

        /// <summary>Sets the current audience for filtering.</summary>
        /// <param name="audience">The current audience string.</param>
        public void SetAudience(string audience)
        {
            if (_currentAudience != audience)
            {
                _currentAudience = audience;
                OnFilterParametersChanged?.Invoke();
                Log.ForContext("Class", nameof(FilterService))
                   .ForContext("Method", nameof(SetAudience))
                   .Debug("Audience set to: {Audience}", audience);
            }
        }

        /// <summary>Adds or removes a tag from the active filters.</summary>
        /// <param name="tag">The tag to add or remove.</param>
        /// <param name="isChecked">True to add, false to remove.</param>
        public void SetTagFilter(string tag, bool isChecked)
        {
            if (isChecked)
            {
                if (_activeTagFilters.Add(tag))
                {
                    OnFilterParametersChanged?.Invoke();
                    Log.ForContext("Class", nameof(FilterService))
                       .ForContext("Method", nameof(SetTagFilter))
                       .Debug("Tag '{Tag}' added to active filters.", tag);
                }
                else
                {
                    Log.ForContext("Class", nameof(FilterService))
                       .ForContext("Method", nameof(SetTagFilter))
                       .Warning("Tag '{Tag}' is already active.", tag);
                }
            }
            else
            {
                if (_activeTagFilters.Remove(tag))
                {
                    OnFilterParametersChanged?.Invoke();
                    Log.ForContext("Class", nameof(FilterService))
                       .ForContext("Method", nameof(SetTagFilter))
                       .Debug("Tag '{Tag}' removed from active filters.", tag);
                }
                else
                {
                    Log.ForContext("Class", nameof(FilterService))
                       .ForContext("Method", nameof(SetTagFilter))
                       .Warning("Tag '{Tag}' was not active, so it could not be removed.", tag);
                }
            }
        }

        /// <summary>Checks if a specific tag is currently active as a filter.</summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>True if the tag is active, false otherwise.</returns>
        public bool IsTagFilterActive(string tag) => _activeTagFilters.Contains(tag, StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets all unique tags available across the entire content metadata.</summary>
        /// <returns>A list of all unique tags.</returns>
        public List<string> GetAllAvailableTags()
            => _allContentMetadata
                    .SelectMany(m => m.Tags ?? Enumerable.Empty<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(tag => tag)
                    .ToList();

        /// <summary>
        /// Gets all currently active tag filters.
        /// </summary>
        /// <returns>An enumerable of active tag filter strings.</returns>
        public IEnumerable<string> GetAllActiveTagFilters()
            => _activeTagFilters.ToList();

        /// <summary>Applies all current filters (audience, search, tags) to the content metadata.</summary>
        /// <returns>A filtered list of content metadata.</returns>
        public List<ContentMetadata> GetFilteredContent()
        {
            IEnumerable<ContentMetadata> query = _allContentMetadata;

            if (!string.IsNullOrEmpty(_currentAudience))
            {
                query = query.Where(item => item.TargetAudiences.Contains("all", StringComparer.OrdinalIgnoreCase) ||
                                            item.TargetAudiences.Contains(_currentAudience, StringComparer.OrdinalIgnoreCase));
            }

            if (_activeTagFilters.Any())
            {
                query = query.Where(content =>
                    content.Tags != null &&
                    content.Tags.Any(tag => _activeTagFilters.Contains(tag, StringComparer.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(_currentSearchText))
            {
                query = query.Where(content =>
                    content.Title.ToLowerInvariant().Contains(_currentSearchText) ||
                    content.Description.ToLowerInvariant().Contains(_currentSearchText) ||
                    (content.Tags != null && content.Tags.Any(tag => tag.ToLowerInvariant().Contains(_currentSearchText)))
                );
            }

            return query.OrderByDescending(item => item.PublishDate).ToList();
        }

        /// <summary>Clears all active tag filters in the FilterService.</summary>
        public void ClearTagFilters()
        {
            if (_activeTagFilters.Any())
            {
                _activeTagFilters.Clear();
                OnFilterParametersChanged?.Invoke();
                Log.ForContext("Class", nameof(FilterService))
                   .ForContext("Method", nameof(ClearTagFilters))
                   .Debug("All tag filters cleared.");
            }
        }
    }
}
