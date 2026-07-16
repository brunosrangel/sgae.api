using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sgae.API.Middlewares;
using System.Text.Json;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pelo registro e exposição do endpoint de Health Checks,
/// com payload estruturado em JSON seguindo as práticas corporativas.
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddSgaeHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("PostgreSQL_Database_Check", failureStatus: HealthStatus.Unhealthy);

        return services;
    }

    public static WebApplication MapSgaeHealthCheckEndpoint(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    totalDurationMilliseconds = report.TotalDuration.TotalMilliseconds,
                    timestampUtc = DateTime.UtcNow,
                    dependencies = report.Entries.Select(entry => new
                    {
                        dependency = entry.Key,
                        status = entry.Value.Status.ToString(),
                        durationMilliseconds = entry.Value.Duration.TotalMilliseconds,
                        description = entry.Value.Description,
                        errorMessage = entry.Value.Exception?.Message
                    })
                };

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        });

        return app;
    }
}
