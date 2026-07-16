using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis por otimizações de performance da API,
/// como a compressão de respostas HTTP para payloads grandes.
/// </summary>
public static class PerformanceExtensions
{
    public static IServiceCollection AddSgaeResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }
}
