using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Homepage.Components.Base;

public partial class BaseComponent : ComponentBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    [Inject] protected HttpClient Http { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }
    [Inject] protected ISnackbar Snackbar { get; set; }
}
