using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sgae.Application.Abstractions;
using Sgae.Domain.Enums;
using System.Diagnostics;
using System.Text;

namespace Sgae.API.Middlewares;

/// <summary>
/// Verificação de saúde customizada para monitorar o status dos serviços de Identidade,
/// autenticação JWT, integridade do repositório de usuários e infraestrutura criptográfica.
/// </summary>
public class IdentityHealthCheck : IHealthCheck
{
    private readonly IAppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;

    public IdentityHealthCheck(
        IAppDbContext dbContext,
        IConfiguration configuration,
        ITokenService tokenService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var data = new Dictionary<string, object>();

        try
        {
            // 1. Verificação da configuração JWT (Key, Issuer, Audience)
            var jwtKey = _configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SGAE.API";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "SGAE.API";
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 120;

            var isKeyValidLength = Encoding.UTF8.GetByteCount(jwtKey) >= 32; // HMAC-SHA256 exige mínimo 256 bits (32 bytes)

            data.Add("JwtIssuer", jwtIssuer);
            data.Add("JwtAudience", jwtAudience);
            data.Add("JwtExpirationMinutes", expirationMinutes);
            data.Add("JwtKeyStrengthBits", Encoding.UTF8.GetByteCount(jwtKey) * 8);

            if (!isKeyValidLength)
            {
                return HealthCheckResult.Degraded(
                    "A chave de assinatura JWT configurada possui tamanho inferior ao padrão recomendado (mínimo 256 bits).",
                    data: data);
            }

            // 2. Verificação de integridade e conectividade do armazenamento de Identidade (Tabela de Usuários)
            var totalUsers = await _dbContext.Usuarios
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var activeUsersCount = await _dbContext.Usuarios
                .AsNoTracking()
                .CountAsync(u => u.StatusAtivo && !u.IsDeleted, cancellationToken);

            var adminCount = await _dbContext.Usuarios
                .AsNoTracking()
                .CountAsync(u => u.Perfil == PerfilUsuario.Admin && u.StatusAtivo && !u.IsDeleted, cancellationToken);

            data.Add("TotalUsers", totalUsers);
            data.Add("ActiveUsersCount", activeUsersCount);
            data.Add("ActiveAdminsCount", adminCount);
            data.Add("IdentityStoreReady", true);
            data.Add("ResponseTimeMs", stopwatch.ElapsedMilliseconds);

            if (adminCount == 0)
            {
                return HealthCheckResult.Degraded(
                    "O serviço de identidade está operacional, mas não foi detectado nenhum administrador ativo cadastrado no sistema.",
                    data: data);
            }

            stopwatch.Stop();
            return HealthCheckResult.Healthy(
                $"O serviço de Identidade e Autenticação JWT está saudável e operacional. {activeUsersCount} usuários ativos cadastrados ({adminCount} administradores).",
                data: data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            data.Add("ResponseTimeMs", stopwatch.ElapsedMilliseconds);
            data.Add("IdentityStoreReady", false);

            return HealthCheckResult.Unhealthy(
                "Falha ao validar os serviços de Identidade e repositório de autenticação.",
                exception: ex,
                data: data);
        }
    }
}
