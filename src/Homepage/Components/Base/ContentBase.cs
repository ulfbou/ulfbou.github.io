using Homepage.Common.Models;
using Homepage.Common.Services;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Serilog;

namespace Homepage.Components.Base
{
    /// <summary>
    /// Base component for content-related Blazor components.
    /// Handles loading, filtering, and rendering content metadata and markdown for the current audience.
    /// </summary>
    public abstract class ContentBase : BaseComponent, IDisposable
    {
#pragma warning disable CS8618
        /// <summary>Service for loading and rendering markdown content.</summary>
        [Inject] protected ContentMarkdownService ContentMarkdownService { get; set; }

        /// <summary>Service for managing the current audience context.</summary>
        [Inject] protected AudienceContextService AudienceService { get; set; }
#pragma warning restore CS8618

        /// <summary>The filtered list of content metadata for the current audience.</summary>
        protected List<ContentMetadata> ContentList { get; set; } = new List<ContentMetadata>();

        /// <summary>The rendered HTML content for the selected content item.</summary>
        protected string ContentHtml { get; set; } = string.Empty;

        /// <summary>Indicates whether content is currently being loaded.</summary>
        protected bool IsLoading { get; set; } = false;

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            AudienceService.OnAudienceChanged += async () => await InvokeAsync(LoadMetadataAndFilter);
            await LoadMetadataAndFilter();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            AudienceService.OnAudienceChanged -= async () => await InvokeAsync(LoadMetadataAndFilter);
        }

        /// <summary>
        /// Loads all content metadata and filters it for the current audience.
        /// Updates <see cref="ContentList"/> and handles loading state and errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task LoadMetadataAndFilter()
        {
            var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadMetadataAndFilter");

            try
            {
                IsLoading = true;
                logger.Information("Loading all content metadata.");
                var allContent = await ContentMarkdownService.GetContentMetadataAsync();
                var currentAudience = AudienceService.CurrentAudience;
                ContentList = allContent
                    .Where(item => item.TargetAudiences.Contains(currentAudience, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(item => item.PublishDate)
                    .ToList();
                logger.Information("Loaded and filtered {Count} content items for audience '{Audience}'.", ContentList.Count, currentAudience);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load and filter content metadata.");
                Snackbar.Add($"Failed to load content: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
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
                logger.Information("Loading markdown content for: {ContentPath}", contentItem.ContentPath);
                var markdown = await ContentMarkdownService.GetMarkdownContentAsync(contentItem.ContentPath);
                logger.Information("Loaded markdown content for: {ContentPath}", contentItem.ContentPath);
                ContentHtml = await ContentMarkdownService.RenderMarkdownToHtmlAsync(markdown);
                logger.Information("Converted markdown to HTML.");
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
            _ = LoadMetadataAndFilter();
        }
    }
}
