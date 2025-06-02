using Microsoft.AspNetCore.Components;
// REMOVED: using Microsoft.AspNetCore.Components.Web;
using Homepage.Common.Models;
using Homepage.Common.Services;
using MudBlazor;

namespace Homepage.Components
{
    public partial class ContentCard : ComponentBase
    {
        [Parameter]
        public ContentMetadata Content { get; set; } = new();

        [Parameter]
        public EventCallback OnClick { get; set; }

        [Parameter]
        public string? Class { get; set; }

        private bool IsPinned { get; set; }

        // If you encounter NullReferenceException for LocalStorageService or Snackbar,
        // uncomment and add the following [Inject] properties:
        // [Inject] public LocalStorageService LocalStorageService { get; set; } = default!;
        // [Inject] public ISnackbar Snackbar { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var pinnedSlugs = await LocalStorageService.GetPinnedProjectTitlesAsync();
            IsPinned = pinnedSlugs.Contains(Content.Slug);
        }

        private async Task TogglePin()
        {
            var pinnedSlugs = await LocalStorageService.GetPinnedProjectTitlesAsync();

            if (IsPinned)
            {
                pinnedSlugs.Remove(Content.Slug);
                Snackbar.Add($"'{Content.Title}' unpinned.", Severity.Info);
            }
            else
            {
                pinnedSlugs.Add(Content.Slug);
                Snackbar.Add($"'{Content.Title}' pinned!", Severity.Success);
            }

            await LocalStorageService.SetPinnedProjectTitlesAsync(pinnedSlugs);
            IsPinned = !IsPinned;
        }
    }
}
