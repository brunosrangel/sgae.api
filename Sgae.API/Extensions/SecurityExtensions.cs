using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Sgae.Domain.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pelas configurações de segurança da API:
/// autenticação JWT com tratamento centralizado de desafios/rejeições (RFC 7807),
/// políticas de autorização RBAC (Roles e Claims), CORS corporativo e Rate Limiting.
/// </summary>
public static class SecurityExtensions
{
    public static IServiceCollection AddSgaeJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "SGAE.API";
        var jwtAudience = configuration["Jwt:Audience"] ?? "SGAE.API";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero, // Elimina tolerância de relógio para expiração precisa
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier
            };

            // Tratamento centralizado e padronizado (RFC 7807) para desafios e recusas de autenticação
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    // Suprime o cabeçalho padrão para emitir o corpo JSON ProblemDetails
                    context.HandleResponse();

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                        Detail = string.IsNullOrWhiteSpace(context.ErrorDescription)
                            ? "Autenticação obrigatória. Forneça um token JWT Bearer válido no cabeçalho Authorization."
                            : context.ErrorDescription,
                        Instance = context.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                    await context.Response.WriteAsync(json);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Forbidden",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                        Detail = "Acesso negado. Seu perfil de usuário não possui as permissões necessárias para acessar este recurso.",
                        Instance = context.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                    await context.Response.WriteAsync(json);
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Configuração de RBAC com Roles e Políticas Estruturadas
        services.AddAuthorization(options =>
        {
            // Políticas por Role individual
            options.AddPolicy(SgaePolicies.AdminOnly, policy =>
                policy.RequireRole(SgaeRoles.Admin));

            options.AddPolicy(SgaePolicies.SacerdoteOnly, policy =>
                policy.RequireRole(SgaeRoles.Sacerdote));

            options.AddPolicy(SgaePolicies.SecretariaOnly, policy =>
                policy.RequireRole(SgaeRoles.Secretaria));

            options.AddPolicy(SgaePolicies.ConsulenteOnly, policy =>
                policy.RequireRole(SgaeRoles.Consulente, SgaeRoles.Admin));

            // Políticas por Domínio Funcional
            options.AddPolicy(SgaePolicies.EquipePastoral, policy =>
                policy.RequireRole(SgaeRoles.Admin, SgaeRoles.Sacerdote, SgaeRoles.Secretaria));

            options.AddPolicy(SgaePolicies.AtendimentoEspiritual, policy =>
                policy.RequireRole(SgaeRoles.Admin, SgaeRoles.Sacerdote));

            options.AddPolicy(SgaePolicies.RecepcaoEAgendamento, policy =>
                policy.RequireRole(SgaeRoles.Admin, SgaeRoles.Secretaria, SgaeRoles.Sacerdote));
        });

        return services;
    }

    /// <summary>
    /// Política de CORS corporativa para integração segura com o SGAE Frontend.
    /// </summary>
    public static IServiceCollection AddSgaeCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("SgaeCorsPolicy", policy =>
            {
                policy.WithOrigins("https://sgae-ui.vercel.app")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Rate Limiting nativo do ASP.NET Core:
    /// - Limitador global por IP para todas as rotas (100 req/min)
    /// - Política 'AuthPolicy' estrita anti-brute force para endpoints de login, cadastro e primeiro acesso (10 req/min)
    /// - Política 'AtendimentosPolicy' dedicada para serviços de atendimento e evolução (30 req/min)
    /// </summary>
    public static IServiceCollection AddSgaeRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Limitador Global particionado por IP do cliente
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = ResolveClientIpAddress(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // Política dedicada para Prevenção de Ataques de Força Bruta (Brute-Force / Credential Stuffing)
            // Aplica-se aos endpoints críticos de autenticação, registro, primeiro acesso e alteração de senha
            options.AddSlidingWindowLimiter(policyName: "AuthPolicy", limOptions =>
            {
                limOptions.PermitLimit = 10; // Máximo de 10 tentativas por minuto por IP
                limOptions.Window = TimeSpan.FromMinutes(1);
                limOptions.SegmentsPerWindow = 6; // Segmentos de 10s para decaimento suave
                limOptions.QueueLimit = 0; // Sem fila para barrar requisições excedentes instantaneamente
                limOptions.AutoReplenishment = true;
            });

            // Política dedicada para os serviços de atendimento e evolução
            options.AddFixedWindowLimiter(policyName: "AtendimentosPolicy", limOptions =>
            {
                limOptions.PermitLimit = 30; // 30 requisições permitidas por janela
                limOptions.Window = TimeSpan.FromMinutes(1);
                limOptions.QueueLimit = 2; // Pequena fila para suavizar picos de acesso
                limOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limOptions.AutoReplenishment = true;
            });

            // Resposta de rejeição padronizada seguindo o padrão RFC 7807 (ProblemDetails) com Retry-After
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                context.HttpContext.Response.Headers.Append("Retry-After", "60");

                var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                var isAuthPath = path.Contains("/auth", StringComparison.OrdinalIgnoreCase) || 
                                 path.Contains("/usuario", StringComparison.OrdinalIgnoreCase);

                var detail = isAuthPath
                    ? "Muitas tentativas de autenticação/cadastro detectadas a partir deste endereço IP. Por favor, aguarde 1 minuto antes de tentar novamente (Proteção Anti-Brute Force)."
                    : "O limite de requisições por minuto foi excedido. Por favor, tente novamente mais tarde.";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = "Too Many Requests - Rate Limit Exceeded",
                    Detail = detail,
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                problemDetails.Extensions.Add("retryAfterSeconds", 60);

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
            };
        });

        return services;
    }

    private static string ResolveClientIpAddress(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
        {
            var ip = forwardedHeader.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
    }
}
