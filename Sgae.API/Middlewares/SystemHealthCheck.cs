using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sgae.API.Middlewares;

/// <summary>
/// Verificação de saúde customizada para monitorar o status do sistema, integridade do runtime e consumo de memória.
/// </summary>
public class SystemHealthCheck : IHealthCheck
{
    private static readonly DateTime AppStartTime = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryAllocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
            var memoryAllocatedMb = Math.Round(memoryAllocatedBytes / (1024.0 * 1024.0), 2);
            var uptime = DateTime.UtcNow - AppStartTime;
            var currentProcess = Process.GetCurrentProcess();

            var data = new Dictionary<string, object>
            {
                { "AllocatedMemoryMB", memoryAllocatedMb },
                { "ThreadCount", currentProcess.Threads.Count },
                { "ProcessorCount", Environment.ProcessorCount },
                { "OsVersion", Environment.OSVersion.ToString() },
                { "DotNetVersion", Environment.Version.ToString() },
                { "UptimeHours", Math.Round(uptime.TotalHours, 2) },
                { "UptimeFormatted", uptime.ToString(@"dd\.hh\:mm\:ss") },
                { "MachineName", Environment.MachineName }
            };

            // Se o consumo de memória for excessivo (> 2GB para o container), emitir Degraded
            if (memoryAllocatedMb > 2048)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"O sistema está consumindo alta quantidade de memória ({memoryAllocatedMb} MB).",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"O sistema está operando normalmente. Uptime: {uptime:hh\\:mm\\:ss}, Memória: {memoryAllocatedMb} MB.",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Falha ao coletar telemetria e métricas do sistema operacional.",
                exception: ex));
        }
    }
}
