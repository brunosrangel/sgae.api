using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using Sgae.Application;
using Sgae.Application.Abstractions;
using Sgae.Infrastructure.Persistence;
using Sgae.API.Middlewares;
using Sgae.Domain.Repositories;
using Sgae.Infrastructure.Persistence.Repositories;

using Serilog;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Configuração estruturada e dedicada do Serilog enriquecido com Correlation ID para rastreabilidade
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [ID-Correlacao: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/sgae-log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [ID-Correlacao: {CorrelationId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Adiciona Serviços das Camadas de Arquitetura Clean
builder.Services.AddApplication(); // Registra o MediatR e pipeline CQRS via método de extensão da Application

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SGAE API - Sistema de Gestão de Atendimento Especializado",
        Description = "API RESTful corporativa para gestão da jornada do consulente, estruturada sobre os princípios de Clean Architecture e DDD.\n\n" +
                      "### 📌 Módulos Funcionais da API:\n" +
                      "- **Módulo 1: Captação (Leads)** - Registro inicial e controle de captação de consulentes.\n" +
                      "- **Módulo 2: Agendamentos** - Gestão do fluxo de datas, status e valores de consultas.\n" +
                      "- **Módulo 3: Perfil do Consulente** - Enriquecimento sócio-demográfico e qualificação personalizada.\n" +
                      "- **Módulo 4: Atendimento Espiritual e Acompanhamento** - Registro de atendimentos e monitoramento continuado de evolução.\n\n" +
                      "*Padrões adotados: CQRS, Mediator Pipeline Behaviors, FluentValidation e Soft-Delete.*",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Arquitetura e Engenharia SGAE",
            Email = "arquitetura@sgae.com.br"
        }
    });

    // Agrupamento lógico e ordenação de rotas por Módulo Funcional para melhor DX (Developer Experience)
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        if (controllerName != null)
        {
            if (controllerName.Contains("Lead", StringComparison.OrdinalIgnoreCase))
                return new[] { "1. Módulo de Captação (Leads)" };
            if (controllerName.Contains("Agendamento", StringComparison.OrdinalIgnoreCase))
                return new[] { "2. Módulo de Agendamentos" };
            if (controllerName.Contains("Perfil", StringComparison.OrdinalIgnoreCase))
                return new[] { "3. Módulo de Perfil do Consulente" };
            if (controllerName.Contains("Atendimento", StringComparison.OrdinalIgnoreCase) || controllerName.Contains("Acompanhamento", StringComparison.OrdinalIgnoreCase))
                return new[] { "4. Módulo de Atendimento Espiritual e Acompanhamento" };

            return new[] { controllerName };
        }

        return new[] { "Geral" };
    });

    // Configuração de autenticação Bearer para o Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT de autorização Bearer no formato: Bearer {seu_token_aqui}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Carrega dinamicamente os arquivos XML compilados para documentar as DTOs e ações nos controllers de forma autogerada
    var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath);
    }

    var appXmlFile = "Sgae.Application.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
    if (File.Exists(appXmlPath))
    {
        options.IncludeXmlComments(appXmlPath);
    }
});

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração robusta do Redis Distributed Cache com fallback em-memória para ambientes de dev ou test sem infraestrutura Redis
var redisConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"] 
    ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "Sgae_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Configuração de Compressão de Resposta para melhorar a performance de payloads grandes da API
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Configuração de Rate Limiting nativa do ASP.NET Core para mitigar ataques de força bruta e garantir uso justo de recursos
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Configura o limitador global que protege todos os endpoints contra abuso usando o IP do cliente como partição
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Política específica de Janela Fixa (Fixed Window) dedicada para os Serviços de Atendimento ("attendance management services")
    options.AddFixedWindowLimiter(policyName: "AtendimentosPolicy", limOptions =>
    {
        limOptions.PermitLimit = 30; // 30 requisições permitidas por janela
        limOptions.Window = TimeSpan.FromMinutes(1); // Janela fixa de 1 minuto
        limOptions.QueueLimit = 2; // Pequena fila para suavizar picos de acessos sem estresse no servidor
        limOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limOptions.AutoReplenishment = true;
    });

    // Customiza a resposta de rejeição para seguir o padrão corporativo RFC 7807 (Problem Details)
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = "Too Many Requests",
            Detail = "O limite de requisições foi excedido para o serviço de atendimento. Por favor, tente novamente mais tarde.",
            Instance = context.HttpContext.Request.Path
        };

        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
    };
});

// Configuração estruturada e segura do JWT Authentication para proteger os endpoints da API
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SGAE.API",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SGAE.API",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!"
        ))
    };
});

builder.Services.AddAuthorization();

// Registro das implementações físicas da camada de Infrastructure
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IPerfilConsulenteRepository, PerfilConsulenteRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IAtendimentoEspiritualRepository, AtendimentoEspiritualRepository>();
builder.Services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepository>();
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Registro do serviço de Health Checks com checker para banco de dados relacional
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("PostgreSQL_Database_Check", failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

// Captura requisições e respostas HTTP de forma estruturada no pipeline
app.UseSerilogRequestLogging();

// Ativa a compressão de respostas HTTP para melhor performance de payload
app.UseResponseCompression();

// Ativa o Middleware de ID de Correlação de forma prioritária para rastreio end-to-end
app.UseMiddleware<CorrelationIdMiddleware>();

// ATIVE O MIDDLEWARE DE EXCEÇÕES GLOBAL (RFC 7807)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ativa rate limiting com políticas de segurança por IP do cliente
app.UseRateLimiter();

// Ativa autenticação baseada em JWT Bearer e autorização corporativa nas rotas
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Mapeamento do endpoint de Health Checks com payload estruturado em JSON seguindo melhores práticas corporativas
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

        // Usa serializer com formatação identada para melhor legibilidade
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
});

// Executa a inicialização e o seeding automático do banco de dados relacional na carga inicial do SGAE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.InitializeAndSeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
        logger.LogCritical(ex, "SGAE Core: Falha estrutural crítica ao executar a inicialização e o seeding automático do banco de dados.");
    }
}

app.Run();