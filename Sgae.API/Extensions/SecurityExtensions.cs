using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pelas configurações de segurança da API:
/// autenticação JWT, política de CORS e Rate Limiting.
/// </summary>
public static class SecurityExtensions
{
    public static IServiceCollection AddSgaeJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
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
                ValidIssuer = configuration["Jwt:Issuer"] ?? "SGAE.API",
                ValidAudience = configuration["Jwt:Audience"] ?? "SGAE.API",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!"))
            };
        });

        services.AddAuthorization();

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
    /// Rate Limiting nativo do ASP.NET Core: um limitador global por IP e uma política
    /// dedicada e mais restritiva para os serviços de atendimento.
    /// </summary>
    public static IServiceCollection AddSgaeRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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

            options.AddFixedWindowLimiter(policyName: "AtendimentosPolicy", limOptions =>
            {
                limOptions.PermitLimit = 30; // 30 requisições permitidas por janela
                limOptions.Window = TimeSpan.FromMinutes(1); // Janela fixa de 1 minuto
                limOptions.QueueLimit = 2; // Pequena fila para suavizar picos de acesso
                limOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limOptions.AutoReplenishment = true;
            });

            // Resposta de rejeição seguindo o padrão RFC 7807 (Problem Details)
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
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

        return services;
    }
}
