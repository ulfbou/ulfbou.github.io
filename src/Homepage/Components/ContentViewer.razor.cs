using Microsoft.AspNetCore.Components;

namespace Homepage.Components
{
    public partial class ContentViewer : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public required string ContentTitle { get; set; }
        [Parameter] public required string Url { get; set; }
        private string ContentHtml = string.Empty;

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                var markdown = await Http.GetStringAsync(Url);
                ContentHtml = Markdig.Markdown.ToHtml(markdown);
            }
            catch (Exception ex)
            {
                ContentHtml = $"<p>Error loading content: {ex.Message}</p>";
            }
        }
    }
}
