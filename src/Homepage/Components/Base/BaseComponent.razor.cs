using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Homepage.Components.Base
{
    public partial class BaseComponent : ComponentBase
    {
#pragma warning disable CS8618
        [Inject] protected HttpClient Http { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected ISnackbar Snackbar { get; set; }
#pragma warning restore CS8618
    }
}
