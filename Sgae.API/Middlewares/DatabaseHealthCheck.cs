using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sgae.Infrastructure.Persistence;

namespace Sgae.API.Middlewares;

/// <summary>
/// Verificação de saúde customizada para validar a conectividade, latência e status operacional do banco de dados relacional.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var data = new Dictionary<string, object>();

        try
        {
            var providerName = _dbContext.Database.ProviderName ?? "Unknown";
            var databaseName = _dbContext.Database.IsRelational()
                ? _dbContext.Database.GetDbConnection().Database
                : "InMemoryDatabase";

            data.Add("Provider", providerName);
            data.Add("DatabaseName", databaseName ?? "Unknown");

            // Valida conectividade real executando verificação de conexão
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            data.Add("ResponseTimeMs", elapsedMs);

            if (canConnect)
            {
                if (elapsedMs > 1500)
                {
                    return HealthCheckResult.Degraded(
                        $"O banco de dados está respondendo com alta latência ({elapsedMs} ms).",
                        data: data);
                }

                return HealthCheckResult.Healthy(
                    $"O banco de dados relacional ({providerName}) está respondendo perfeitamente ({elapsedMs} ms).",
                    data: data);
            }

            return HealthCheckResult.Unhealthy(
                "O banco de dados relacional recusou a conexão ou está inacessível.",
                data: data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            data.Add("ResponseTimeMs", stopwatch.ElapsedMilliseconds);

            return HealthCheckResult.Unhealthy(
                "Ocorreu uma falha inesperada de conectividade com o banco de dados.",
                exception: ex,
                data: data);
        }
    }
}
