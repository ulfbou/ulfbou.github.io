namespace Saos.Interop;

/// <summary>
/// Event args carrying the new theme value, emitted with
/// <c>saos:kernel:theme-changed</c> (SAOS IPC v1 §5.2).
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    /// <summary>The new theme identifier, e.g. <c>"dark"</c> or <c>"light"</c>.</summary>
    public string Theme { get; }

    public ThemeChangedEventArgs(string theme)
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }
}

/// <summary>
/// Event args carrying the <c>saos:kernel:ready</c> payload (§5.1).
/// </summary>
public sealed class KernelReadyEventArgs : EventArgs
{
    public string Theme { get; }
    public IReadOnlyList<string> Capabilities { get; }
    public IReadOnlyList<string> Apps { get; }

    public KernelReadyEventArgs(string theme, IReadOnlyList<string> capabilities, IReadOnlyList<string> apps)
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
        Apps = apps ?? throw new ArgumentNullException(nameof(apps));
    }
}
