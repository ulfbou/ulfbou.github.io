using Microsoft.AspNetCore.Components;

using Homepage.Common.Models;

namespace Homepage.Components;

public partial class ContentCard
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public required ContentItem Content { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
}
