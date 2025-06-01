using Homepage.Common.Models;

using System.Globalization;

namespace Homepage.Layout
{
    public partial class NavMenu : Microsoft.AspNetCore.Components.ComponentBase
    {
        private List<ContentMetadata>? _contentMetadata;

        protected override async Task OnInitializedAsync()
        {
            AudienceService.OnAudienceChanged += OnAudienceChangedHandler;
            await LoadContentMetadata();
        }

        private async void OnAudienceChangedHandler()
        {
            await LoadContentMetadata();
        }

        private async Task LoadContentMetadata()
        {
            _contentMetadata = await ContentMarkdownService.GetContentMetadataAsync();
            StateHasChanged();
        }

        private void OnAudienceOptionChanged(string newAudience)
        {
            AudienceService.SetAudience(newAudience);
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}
