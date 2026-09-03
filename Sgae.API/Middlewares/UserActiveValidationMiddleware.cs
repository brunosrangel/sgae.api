using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;

namespace Sgae.API.Middlewares;

/// <summary>
/// Middleware de segurança responsável por validar se o usuário autenticado via JWT
/// continua com o status ativo no banco de dados em todas as requisições autenticadas.
/// Rejeita imediatamente requisições de usuários desativados ou excluídos com RFC 7807 ProblemDetails.
/// </summary>
public class UserActiveValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserActiveValidationMiddleware> _logger;

    public UserActiveValidationMiddleware(RequestDelegate next, ILogger<UserActiveValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAppDbContext dbContext)
    {
        // Se a requisição não estiver autenticada, prossegue normalmente para o pipeline de autorização avaliar
        if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await RejectInactiveUserAsync(context, "Identificador de usuário inválido nas credenciais da sessão.", StatusCodes.Status401Unauthorized);
            return;
        }

        // Consulta de alta performance para verificar status do usuário
        var usuario = await dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.StatusAtivo, u.IsDeleted, u.Email })
            .FirstOrDefaultAsync();

        if (usuario == null || usuario.IsDeleted)
        {
            _logger.LogWarning("[Security] Tentativa de acesso com token de usuário inexistente ou excluído: {UserId}", userId);
            await RejectInactiveUserAsync(context, "A conta do usuário não foi encontrada ou foi revogada.", StatusCodes.Status401Unauthorized);
            return;
        }

        if (!usuario.StatusAtivo)
        {
            _logger.LogWarning("[Security] Tentativa de acesso bloqueada para usuário inativo: {Email} ({UserId})", usuario.Email, userId);
            await RejectInactiveUserAsync(context, "A conta do usuário está inativa no sistema. Entre em contato com a administração para reativação.", StatusCodes.Status403Forbidden);
            return;
        }

        await _next(context);
    }

    private static async Task RejectInactiveUserAsync(HttpContext context, string detail, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status403Forbidden ? "Forbidden Account Status" : "Unauthorized Account",
            Type = statusCode == StatusCodes.Status403Forbidden
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                : "https://tools.ietf.org/html/rfc7235#section-3.1",
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("traceId", traceId);

        if (context.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId) && !string.IsNullOrEmpty(correlationId))
        {
            problemDetails.Extensions.Add("correlationId", correlationId.ToString());
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }
}
