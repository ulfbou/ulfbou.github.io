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
#pragma warning disable CS8618
        [Inject] protected HttpClient Http { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected ISnackbar Snackbar { get; set; }
#pragma warning restore CS8618

        public abstract class ContentBase : BaseComponent
        {
#pragma warning disable CS8618
            [Inject] protected ContentMarkdownService ContentMarkdownService { get; set; }
            [Inject] protected AudienceContextService AudienceService { get; set; }
#pragma warning restore CS8618
            protected List<ContentMetadata> ContentList { get; set; } = new List<ContentMetadata>();
            protected bool IsLoading { get; set; } = false;
            protected string MetadataUrl => "content/metadata.json";
            protected string ContentDirectory => "content";

            protected override async Task OnInitializedAsync()
            {
                await LoadMetadataAndFilter();
            }

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
        }
    }
}
