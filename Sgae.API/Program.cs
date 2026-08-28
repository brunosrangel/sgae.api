using Serilog;
using Sgae.API.Extensions;
using Sgae.Application;

SerilogExtensions.ConfigureBootstrapLogger();

try
{
    Log.Information("Iniciando o host de inicialização da SGAE API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSgaeSerilog();

    // Camadas de Arquitetura Clean
    builder.Services.AddApplication(); // MediatR e pipeline CQRS via método de extensão da Application

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new Sgae.Application.Common.JsonConverters.FlexibleEnumConverterFactory());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
    builder.Services.AddEndpointsApiExplorer();

    // Documentação, persistência, cache, performance, segurança e observabilidade
    builder.Services.AddSgaeSwagger();
    builder.Services.AddSgaePersistence(builder.Configuration);
    builder.Services.AddSgaeCaching(builder.Configuration);
    builder.Services.AddSgaeResponseCompression();
    builder.Services.AddSgaeRateLimiting();
    builder.Services.AddSgaeJwtAuthentication(builder.Configuration);
    builder.Services.AddSgaeCorsPolicy();
    builder.Services.AddSgaeHealthChecks();

    var app = builder.Build();

    app.UseSgaeRequestPipeline();
    app.MapControllers();
    app.MapSgaeHealthCheckEndpoint();

    await app.SeedSgaeDatabaseAsync();

    Log.Information("Host da SGAE API configurado com sucesso. Executando o pipeline...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O host da aplicação SGAE API falhou inesperadamente durante a fase de inicialização ou execução.");
    throw;
}
finally
{
    Log.Information("Finalizando o host da SGAE API de forma segura...");
    Log.CloseAndFlush();
}
