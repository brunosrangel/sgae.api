using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sgae.API.Middlewares;
using System.Text.Json;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pelo registro e exposição dos endpoints de Health Checks corporativos,
/// monitorando a saúde e prontidão do Banco de Dados, Serviços de Identidade (Auth/JWT) e Runtime.
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddSgaeHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "Database_PostgreSQL",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "ready", "health" })
            .AddCheck<IdentityHealthCheck>(
                "Identity_Auth_Service",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "identity", "auth", "jwt", "ready", "health" })
            .AddCheck<SystemHealthCheck>(
                "System_Runtime",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "system", "live", "ready", "health" });

        return services;
    }

    public static WebApplication MapSgaeHealthCheckEndpoint(this WebApplication app)
    {
        // Endpoint geral de saúde (/health e /api/health)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        // Endpoint de Readiness (/health/ready) - verifica se dependências críticas (DB, Identidade) estão prontas
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        // Endpoint de Liveness (/health/live) - verifica se a aplicação está viva e respondendo
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        return app;
    }

    public static async Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            overallStatus = report.Status == HealthStatus.Healthy ? "Healthy" :
                            report.Status == HealthStatus.Degraded ? "Degraded" : "Unhealthy",
            totalDurationMilliseconds = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            timestampUtc = DateTime.UtcNow,
            services = report.Entries.Select(entry => new
            {
                service = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMilliseconds = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
                description = entry.Value.Description,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
                errorMessage = entry.Value.Exception?.Message
            })
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
