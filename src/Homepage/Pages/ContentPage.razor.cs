using Homepage.Common.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Serilog;

namespace Homepage.Pages
{
    public partial class ContentPage : Homepage.Components.Base.ContentBase
    {
        [Parameter]
        public string Slug { get; set; } = string.Empty;
        private string? _htmlContent;
        private string? _tocHtmlContent;
        private bool _isLoading = true;
        private ContentMetadata? _currentMetadata;
        private ElementReference _markdownContentContainer;
        private List<TocEntry> _tocEntries = new();

        protected override async Task OnParametersSetAsync()
        {
            _isLoading = true;
            _htmlContent = null;
            _currentMetadata = null;
            _tocHtmlContent = null;

            var allMetadata = await MarkdownService.GetContentMetadataAsync();
            _currentMetadata = allMetadata.FirstOrDefault(m => m.Slug.Equals(Slug, StringComparison.OrdinalIgnoreCase));

            if (_currentMetadata != null)
            {
                var markdown = await MarkdownService.GetMarkdownContentAsync(_currentMetadata.ContentPath);
                (string mainHtml, string generatedTocHtml) = await MarkdownService.RenderMarkdownWithTocAsync(markdown);
                _htmlContent = mainHtml;
                _tocHtmlContent = generatedTocHtml;
            }
            else
            {
                _htmlContent = null;
                _tocHtmlContent = null;
            }

            Log.ForContext("Class: {Name}", GetType().Name)
                .ForContext("Method", "OnParametersSetAsync")
                .ForContext("Slug", Slug)
                .Information("ContentPage.razor.cs: Loaded content for slug '{Slug}'. Metadata found: {MetadataFound}", Slug, _currentMetadata != null);

            _isLoading = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!_isLoading && _htmlContent != null && _markdownContentContainer.Id != null)
            {
                await JSRuntime.InvokeVoidAsync("appJsFunctions.highlightCode", _markdownContentContainer.Id);
                await JSRuntime.InvokeVoidAsync("appJsFunctions.applyLazyLoading", _markdownContentContainer.Id);

                var headings = await JSRuntime.InvokeAsync<List<TocEntry>>("appJsFunctions.getHeadings", _markdownContentContainer.Id);

                if (headings != null && headings.Any())
                {
                    _tocEntries = headings;
                    await this.InvokeAsync(StateHasChanged);
                }
            }
        }

        private async Task ScrollToHeading(string id)
        {
            await JSRuntime.InvokeVoidAsync("appJsFunctions.scrollToElement", id);
        }
    }
}
