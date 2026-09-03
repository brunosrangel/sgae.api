using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável por expor endpoints de monitoramento de integridade e saúde (Health Checks)
/// do banco de dados relacional, serviços de identidade/segurança e runtime da aplicação.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Retorna o relatório completo de saúde de todos os subsistemas da API (Banco de Dados, Identidade, Runtime).
    /// </summary>
    /// <response code="200">A API e todos os seus subsistemas estão saudáveis (Healthy ou Degraded).</response>
    /// <response code="503">Um ou mais subsistemas críticos estão indisponíveis (Unhealthy).</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

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

        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Endpoint de verificação de prontidão (Readiness Probe) para verificar se o banco de dados e a identidade estão operacionais.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"), cancellationToken);

        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(statusCode, new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTime.UtcNow,
            dependencies = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 2),
                description = e.Value.Description
            })
        });
    }

    /// <summary>
    /// Endpoint de verificação de vivacidade (Liveness Probe) para atestar se a API está ativa.
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetLiveness(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("live"), cancellationToken);

        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(statusCode, new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTime.UtcNow,
            uptime = report.Entries.FirstOrDefault(e => e.Key == "System_Runtime").Value.Data.GetValueOrDefault("UptimeFormatted")
        });
    }
}
