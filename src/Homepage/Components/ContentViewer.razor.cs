using Homepage.Components.Base;

using Microsoft.AspNetCore.Components;

using Serilog;

namespace Homepage.Components
{
    public partial class ContentViewer : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Parameter] public required string ContentTitle { get; set; }
        [Parameter] public required string Url { get; set; }
        private string ContentHtml = string.Empty;

        protected override async Task OnParametersSetAsync()
        {
            var logger = Log.ForContext("Class: {Name}", GetType().Name)
                            .ForContext("Method", "OnParametersSetAsync")
                            .ForContext("ContentTitle", ContentTitle)
                            .ForContext("Url", Url);

            try
            {
                var markdown = await Http.GetStringAsync(Url);
                ContentHtml = Markdig.Markdown.ToHtml(markdown);
                logger.Information("Content loaded successfully for {ContentTitle} from {Url}", ContentTitle, Url);
            }
            catch (Exception ex)
            {
                ContentHtml = $"<p>Error loading content: {ex.Message}</p>";
                logger.Error(ex, "Failed to load content for {ContentTitle} from {Url}", ContentTitle, Url);
            }
        }
    }
}
