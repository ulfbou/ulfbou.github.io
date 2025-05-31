using Microsoft.AspNetCore.Components;

using Homepage.Common.Models;

namespace Homepage.Components
{
    public partial class ContentCard
    {
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public required ContentMetadata Content { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }
        [Parameter] public string ContentHtml { get; set; } = string.Empty;

        [Inject] public NavigationManager Navigation { get; set; } = null!;


        private void BackToPreviousPage()
        {
            Navigation.NavigateTo("/", forceLoad: false);
        }
    }
}
