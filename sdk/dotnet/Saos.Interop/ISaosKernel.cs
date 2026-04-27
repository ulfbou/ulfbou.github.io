namespace Saos.Interop;

/// <summary>
/// SAOS IPC v1 Blazor WASM SDK interface — <c>ISaosKernel</c>.
///
/// This is the normative surface exposed to Blazor WASM userland applications.
/// Implementations MUST use the official JavaScript interop bridge
/// (<c>wwwroot/saosInterop.js</c>) to guarantee ABI conformance.
///
/// See SAOS IPC v1 §9.2.
/// </summary>
public interface ISaosKernel
{
    // -------------------------------------------------------------------------
    // Kernel → App events (§5)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Raised once when the kernel has finished bootstrapping
    /// (<c>saos:kernel:ready</c>, §5.1).
    ///
    /// Apps SHOULD NOT emit privileged intents before this event fires.
    /// </summary>
    event EventHandler<KernelReadyEventArgs> KernelReady;

    /// <summary>
    /// Raised whenever the active theme changes
    /// (<c>saos:kernel:theme-changed</c>, §5.2).
    /// </summary>
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    // -------------------------------------------------------------------------
    // App → Kernel syscalls (§6)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Request cross-app navigation (<c>saos:nav:navigate</c>, §6.1).
    ///
    /// Apps MUST use this method instead of manipulating <c>location.href</c>.
    /// </summary>
    /// <param name="path">Destination path, e.g. <c>/tools/</c>.</param>
    /// <param name="mode"><c>"push"</c> (default) or <c>"replace"</c>.</param>
    ValueTask NavigateAsync(string path, string mode = "push");

    /// <summary>
    /// Announce this application to the kernel (<c>saos:app:announce</c>, §8.1).
    ///
    /// Should be called once <see cref="KernelReady"/> has fired.
    /// </summary>
    ValueTask AnnounceAsync(
        string title,
        string entry,
        string[] capabilities,
        string version);

    /// <summary>
    /// Request a user logout (<c>saos:user:logout</c>, §6.2).
    /// </summary>
    ValueTask LogoutAsync();

    /// <summary>
    /// Advisory capability request (<c>saos:capability:request</c>, §6.3).
    /// </summary>
    ValueTask RequestCapabilityAsync(string capability);

    // -------------------------------------------------------------------------
    // Shared memory — read-only access (§7)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Read a kernel-owned value from <c>localStorage</c> (§7.1).
    ///
    /// The key is automatically prefixed with <c>saos:</c>.
    /// Apps MUST NOT write to these keys directly.
    /// </summary>
    ValueTask<TValue?> ReadLocalMemoryAsync<TValue>(string key);

    /// <summary>
    /// Read a kernel-owned value from <c>sessionStorage</c> (§7.1).
    ///
    /// The key is automatically prefixed with <c>saos:</c>.
    /// Apps MUST NOT write to these keys directly.
    /// </summary>
    ValueTask<TValue?> ReadSessionMemoryAsync<TValue>(string key);

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialise the interop bridge and subscribe to kernel events.
    /// Call this once from the root component's <c>OnAfterRenderAsync</c>
    /// on the first render.
    /// </summary>
    ValueTask InitializeAsync();

    /// <summary>
    /// Unsubscribe from all kernel events and release JS references.
    /// </summary>
    ValueTask DisposeAsync();
}
