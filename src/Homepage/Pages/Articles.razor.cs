using Microsoft.AspNetCore.Components;

namespace Homepage.Pages
{
    public partial class Articles : Homepage.Components.Base.ContentBase
    {
        private int _maxItems = 5;

        /// <inheritdoc />
        protected override async Task OnParametersSetAsync()
        {
            _maxItems = 5;
            await base.OnParametersSetAsync();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>Increases the number of items displayed by a fixed increment.</summary>
        private void LoadMore()
        {
            _maxItems += 5;
            StateHasChanged();
        }
    }
}
