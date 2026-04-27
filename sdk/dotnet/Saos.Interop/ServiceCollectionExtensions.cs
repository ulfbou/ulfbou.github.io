using Microsoft.Extensions.DependencyInjection;

namespace Saos.Interop;

/// <summary>
/// DI registration helpers for the SAOS Interop SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISaosKernel"/> as a scoped service using the
    /// supplied <paramref name="sourceId"/> (canonical app id / repository name).
    ///
    /// Usage in <c>Program.cs</c>:
    /// <code>
    /// builder.Services.AddSaosKernel("my-app");
    /// </code>
    /// </summary>
    public static IServiceCollection AddSaosKernel(
        this IServiceCollection services,
        string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("sourceId must be non-empty.", nameof(sourceId));

        services.AddScoped<ISaosKernel>(sp =>
        {
            var js = sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
            return new SaosKernel(js, sourceId);
        });

        return services;
    }
}
