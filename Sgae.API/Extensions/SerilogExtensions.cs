using Serilog;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pela configuração estruturada do Serilog,
/// enriquecido com Correlation ID para rastreabilidade ponta a ponta.
/// </summary>
public static class SerilogExtensions
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [ID-Correlacao: {CorrelationId}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Configura o logger de bootstrap, usado antes do host ser construído,
    /// garantindo que falhas na inicialização também sejam registradas.
    /// </summary>
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .WriteTo.File(
                path: "Logs/sgae-log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate)
            .CreateLogger();
    }

    /// <summary>
    /// Integra o Serilog ao host, permitindo leitura de configuração via appsettings
    /// e enriquecimento a partir dos serviços registrados via DI.
    /// </summary>
    public static IHostBuilder UseSgaeSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
    }
}
