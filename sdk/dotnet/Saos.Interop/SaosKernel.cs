using Microsoft.JSInterop;
using System.Text.Json;

namespace Saos.Interop;

/// <summary>
/// SAOS IPC v1 Blazor WASM SDK — <c>SaosKernel</c> implementation.
///
/// Wraps <c>saos-core</c> via the JavaScript interop bridge
/// (<c>wwwroot/saosInterop.js</c>) to provide an idiomatic C# surface to
/// Blazor WASM userland applications.
///
/// Register via DI using <see cref="ServiceCollectionExtensions.AddSaosKernel"/>.
/// See SAOS IPC v1 §9.2.
/// </summary>
public sealed class SaosKernel : ISaosKernel, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly string _sourceId;
    private DotNetObjectReference<SaosKernel>? _selfRef;
    private bool _initialized;

    /// <inheritdoc/>
    public event EventHandler<KernelReadyEventArgs>? KernelReady;

    /// <inheritdoc/>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public SaosKernel(IJSRuntime js, string sourceId)
    {
        _js = js ?? throw new ArgumentNullException(nameof(js));
        _sourceId = !string.IsNullOrWhiteSpace(sourceId)
            ? sourceId
            : throw new ArgumentException("sourceId must be non-empty.", nameof(sourceId));
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _selfRef = DotNetObjectReference.Create(this);

        // Register .NET callbacks so the JS bridge can invoke them when
        // kernel events are dispatched on window.
        await _js.InvokeVoidAsync("saosInterop.initialize", _selfRef);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            await _js.InvokeVoidAsync("saosInterop.dispose");
        }
        _selfRef?.Dispose();
    }

    // -------------------------------------------------------------------------
    // App → Kernel syscalls (§6)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public ValueTask NavigateAsync(string path, string mode = "push")
        => _js.InvokeVoidAsync(
            "saosInterop.emit",
            _sourceId,
            "nav:navigate",
            new { to = path, mode });

    /// <inheritdoc/>
    public ValueTask AnnounceAsync(
        string title,
        string entry,
        string[] capabilities,
        string version)
        => _js.InvokeVoidAsync(
            "saosInterop.emit",
            _sourceId,
            "app:announce",
            new { title, entry, capabilities, version });

    /// <inheritdoc/>
    public ValueTask LogoutAsync()
        => _js.InvokeVoidAsync(
            "saosInterop.emit",
            _sourceId,
            "user:logout",
            new { });

    /// <inheritdoc/>
    public ValueTask RequestCapabilityAsync(string capability)
        => _js.InvokeVoidAsync(
            "saosInterop.emit",
            _sourceId,
            "capability:request",
            new { capability });

    // -------------------------------------------------------------------------
    // Shared memory — read-only (§7)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async ValueTask<TValue?> ReadLocalMemoryAsync<TValue>(string key)
    {
        var raw = await _js.InvokeAsync<string?>("saosInterop.readLocalMemory", key);
        return Deserialize<TValue>(raw);
    }

    /// <inheritdoc/>
    public async ValueTask<TValue?> ReadSessionMemoryAsync<TValue>(string key)
    {
        var raw = await _js.InvokeAsync<string?>("saosInterop.readSessionMemory", key);
        return Deserialize<TValue>(raw);
    }

    // -------------------------------------------------------------------------
    // Callbacks invoked from JavaScript (§5)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Invoked by the JS bridge when <c>saos:kernel:ready</c> fires (§5.1).
    /// </summary>
    [JSInvokable]
    public void OnKernelReady(string theme, string[] capabilities, string[] apps)
    {
        KernelReady?.Invoke(this, new KernelReadyEventArgs(theme, capabilities, apps));
    }

    /// <summary>
    /// Invoked by the JS bridge when <c>saos:kernel:theme-changed</c> fires (§5.2).
    /// </summary>
    [JSInvokable]
    public void OnThemeChanged(string theme)
    {
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private static TValue? Deserialize<TValue>(string? raw)
    {
        if (raw is null) return default;
        try
        {
            return JsonSerializer.Deserialize<TValue>(raw);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
