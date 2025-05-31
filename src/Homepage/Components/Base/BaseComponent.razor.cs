// --- Homepage.Components.Base/BaseComponent.cs (Modified) ---
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using Serilog;
using Homepage.Common.Models;
using Homepage.Common.Services;
using Homepage.Common.Helpers;

namespace Homepage.Components.Base
{
    public partial class BaseComponent : ComponentBase
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Inject] protected HttpClient Http { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected ISnackbar Snackbar { get; set; }
#pragma warning restore CS8618

        // ContentBase now uses ContentMetadata
        public abstract class ContentBase : BaseComponent
        {
            // Removed ContentContext injection and OnNavigationLocationChanged logic
            // as audience context is now managed by AudienceContextService

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            [Inject] protected ContentService ContentService { get; set; }
            [Inject] protected AudienceContextService AudienceService { get; set; }
#pragma warning restore CS8618

            // ContentList now holds ContentMetadata objects
            protected List<ContentMetadata> ContentList { get; set; } = new List<ContentMetadata>();
            protected string ContentHtml { get; set; } = string.Empty; // Initialize to empty string
            protected bool IsLoading { get; set; } = false;

            // MetadataUrl and ContentDirectory are now managed by ContentService
            // protected string MetadataUrl => "content/metadata.json";
            // protected string ContentDirectory => "content";

            protected override async Task OnInitializedAsync()
            {
                // Subscribe to audience changes to re-filter/re-sort content lists if needed
                AudienceService.OnAudienceChanged += async () => await InvokeAsync(LoadMetadataAndFilter);
                await LoadMetadataAndFilter(); // Initial load
            }

            // Dispose of the subscription to prevent memory leaks
            public void Dispose()
            {
                AudienceService.OnAudienceChanged -= async () => await InvokeAsync(LoadMetadataAndFilter);
            }

            // Consolidated method to load metadata and then filter based on current audience
            protected async Task LoadMetadataAndFilter()
            {
                var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadMetadataAndFilter");
                try
                {
                    IsLoading = true;
                    logger.Information("Loading all content metadata.");
                    // Use ContentService to get all metadata
                    var allContent = await ContentService.GetContentMetadataAsync();

                    // Filter content based on the current audience
                    var currentAudience = AudienceService.CurrentAudience;
                    ContentList = allContent
                        .Where(item => item.TargetAudiences.Contains(currentAudience, StringComparer.OrdinalIgnoreCase))
                        .OrderByDescending(item => item.PublishDate) // Always sort by publish date by default
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
                    StateHasChanged(); // Ensure UI updates after loading/filtering
                }
            }

            // LoadContent now takes ContentMetadata object to get the ContentPath
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
                    // Use ContentService.BaseUri and contentItem.ContentPath
                    var url = $"{ContentService.BaseUri}/{contentItem.ContentPath}";
                    logger.Information("Loading content from: {Url}", url);

                    // ContentService will handle caching and fetching the raw markdown
                    var markdown = await ContentService.GetMarkdownContentAsync(contentItem.ContentPath);

                    logger.Information("Loaded markdown content for: {Url}", url);

                    // MarkdownService will handle rendering with the configured pipeline
                    ContentHtml = await ContentService.RenderMarkdownToHtmlAsync(markdown);
                    logger.Information("Converted markdown to HTML.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to load content from: {ContentPath}", contentItem.ContentPath);
                    Snackbar.Add($"Failed to load content: {ex.Message}", Severity.Error);
                    ContentHtml = $"<p>Error loading content: {ex.Message}</p>"; // Display error in UI
                }
                finally
                {
                    IsLoading = false;
                    StateHasChanged(); // Ensure UI updates
                }
            }

            protected void ResetContent()
            {
                ContentHtml = string.Empty;
                // Re-load metadata to show the list again
                _ = LoadMetadataAndFilter();
            }

            // Removed CompareContentItems, SortMetadata, GetTagsByCategory methods
            // as their logic is now handled by AudienceContextService and direct filtering.
        }
    }
}
