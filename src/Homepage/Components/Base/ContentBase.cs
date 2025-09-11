using Homepage.Common.Models;
using Homepage.Common.Services;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homepage.Components.Base
{
    /// <summary>
    /// Base component for content-related Blazor components.
    /// Handles loading, filtering, and rendering content metadata and markdown for the current audience.
    /// </summary>
    public abstract class ContentBase : BaseComponent, IDisposable
    {
        /// <summary>Service for loading and rendering markdown content.</summary>
        [Inject] protected ContentMarkdownService ContentMarkdownService { get; set; } = default!;

        /// <summary>Service for managing the current audience context.</summary>
        [Inject] protected AudienceContextService AudienceService { get; set; } = default!;

        /// <summary>Service for managing global filtering and search.</summary>
        [Inject] protected FilterService FilterService { get; set; } = default!;

        /// <summary>Service for synchronizing application state with the URL.</summary>
        [Inject] protected IUrlSynchronizationService UrlSynchronizationService { get; set; } = default!;

        /// <summary>Service for managing user activity and preferences.</summary>
        [Inject] protected IUserActivityService UserActivityService { get; set; } = default!;


        /// <summary>The filtered list of content metadata for the current audience, search, and active filters.</summary>
        protected List<ContentMetadata> ContentList { get; set; } = new List<ContentMetadata>();

        /// <summary>The rendered HTML content for the selected content item (used by ContentPage).</summary>
        protected string ContentHtml { get; set; } = string.Empty;

        /// <summary>Indicates whether content is currently being loaded.</summary>
        protected bool IsLoading { get; set; } = false;

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            FilterService.OnFilterParametersChanged += HandleFilterParametersChanged;
            AudienceService.OnAudienceChanged += HandleAudienceChanged;

            // Load all content into FilterService first
            await LoadAllContentIntoFilterService();

            // Parse initial filters from URL and apply them via FilterService
            var initialFilters = UrlSynchronizationService.ParseFiltersFromUrl();
            if (initialFilters.HasValue)
            {
                FilterService.CurrentSearchText = initialFilters.Value.SearchText;
                foreach (var tag in initialFilters.Value.ActiveTags)
                {
                    FilterService.SetTagFilter(tag, true);
                }
                FilterService.SetAudience(initialFilters.Value.Audience); // Apply audience from URL if present
            }
            else
            {
                // If no URL filters, apply default audience filter
                FilterService.SetAudience(AudienceService.CurrentAudience);
            }

            // Apply filters to ContentList (this will be triggered by FilterService events if URL filters were set)
            ApplyFiltersToContentList();

            // Update last visit timestamp
            await UserActivityService.UpdateLastVisitTimestampAsync();
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            FilterService.OnFilterParametersChanged -= HandleFilterParametersChanged;
            AudienceService.OnAudienceChanged -= HandleAudienceChanged;
        }

        /// <summary>Handles audience changes by updating FilterService and re-applying filters.</summary>
        private async void HandleAudienceChanged()
        {
            FilterService.SetAudience(AudienceService.CurrentAudience);
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>Handles changes in any filter parameter (search, tags, audience).</summary>
        private void HandleFilterParametersChanged()
        {
            ApplyFiltersToContentList();

            // Synchronize current filter state to URL
            UrlSynchronizationService.SynchronizeFiltersToUrl(
                FilterService.CurrentSearchText,
                FilterService.GetAllActiveTagFilters(),
                AudienceService.CurrentAudience // Use AudienceService.CurrentAudience for URL sync
            );
            StateHasChanged();
        }

        /// <summary>Loads all content metadata into the FilterService. This should typically happen once.</summary>
        protected async Task LoadAllContentIntoFilterService()
        {
            var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadAllContentIntoFilterService");

            try
            {
                IsLoading = true;
                logger.Debug("Loading all content metadata into FilterService.");
                var allContent = await ContentMarkdownService.GetContentMetadataAsync();
                FilterService.InitializeContent(allContent.OrderByDescending(m => m.PublishDate).ToList());
                logger.Debug("Loaded and initialized FilterService with {Count} total content items.", allContent.Count);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load all content metadata into FilterService.");
                Snackbar.Add($"Failed to load content: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>Gets the filtered content from FilterService and updates ContentList.</summary>
        protected void ApplyFiltersToContentList()
        {
            ContentList = FilterService.GetFilteredContent();
        }

        /// <summary>
        /// Loads and renders the markdown content for a given <see cref="ContentMetadata"/> item.
        /// Updates <see cref="ContentHtml"/> and handles loading state and errors.
        /// </summary>
        /// <param name="contentItem">The content metadata item to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task LoadContent(ContentMetadata contentItem)
        {
            var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadContent");

            if (contentItem == null || string.IsNullOrEmpty(contentItem.ContentPath))
            {
                logger.Warning("Attempted to load content with null item or empty ContentPath.");
                Snackbar.Add("Invalid content item selected.", Severity.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                logger.Debug("Loading markdown content for: {ContentPath}", contentItem.ContentPath);
                var markdown = await ContentMarkdownService.GetMarkdownContentAsync(contentItem.ContentPath);
                logger.Debug("Loaded markdown content for: {ContentPath}", contentItem.ContentPath);
                ContentHtml = await ContentMarkdownService.RenderMarkdownToHtmlAsync(markdown);
                logger.Debug("Converted markdown to HTML.");

                // Record content view activity
                if (contentItem != null && !string.IsNullOrEmpty(contentItem.Slug))
                {
                    await UserActivityService.RecordContentViewAsync(contentItem.Slug);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load content from: {ContentPath}", contentItem.ContentPath);
                Snackbar.Add($"Failed to load content: {ex.Message}", Severity.Error);
                ContentHtml = $"<p>Error loading content: {ex.Message}</p>";
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>Resets the rendered content and reloads the filtered content metadata.</summary>
        protected void ResetContent()
        {
            ContentHtml = string.Empty;
            FilterService.CurrentSearchText = string.Empty;
            FilterService.ClearTagFilters();
            ApplyFiltersToContentList();
        }

        /// <summary>Checks if a specific tag filter is currently active.</summary>
        protected bool IsTagFilterActive(string tag)
        {
            return FilterService.IsTagFilterActive(tag);
        }

        /// <summary>Handles tag filter changes by updating the FilterService.</summary>
        protected void OnFilterChanged(string tag, bool isChecked)
        {
            Log.ForContext("Class: {Name}", GetType().Name)
               .ForContext("Method", "OnFilterChanged")
               .Debug("Tag filter changed: {Tag}, IsChecked: {IsChecked}", tag, isChecked);
            FilterService.GetAllActiveTagFilters();
            FilterService.SetTagFilter(tag, isChecked);
            Log.ForContext("Class: {Name}", GetType().Name)
               .ForContext("Method", "OnFilterChanged")
               .Debug("Tag filter changed: {Tag}, IsChecked: {IsChecked}", tag, isChecked);
        }
    }
}
