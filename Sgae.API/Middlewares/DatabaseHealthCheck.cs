using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sgae.Infrastructure.Persistence;

namespace Sgae.API.Middlewares;

/// <summary>
/// Verificação de saúde customizada para validar a conectividade e status operacional do banco de dados relacional.
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
        try
        {
            // Tenta validar se a conexão físicas ao banco de dados está disponível
            var isDbOnline = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (isDbOnline)
            {
                return HealthCheckResult.Healthy("O Banco de dados relacional PostgreSQL está respondendo e operacional.");
            }

            return HealthCheckResult.Unhealthy("O banco de dados relacional PostgreSQL recusou a conexão ou está inacessível.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ocorreu uma falha inesperada de conectividade com o banco de dados.", ex);
        }
    }
}
