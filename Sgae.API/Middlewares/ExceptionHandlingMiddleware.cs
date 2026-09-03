using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sgae.API.Middlewares;

/// <summary>
/// Middleware global para capturar, logar detalhadamente e normalizar erros em formato ProblemDetails (RFC 7807).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, type, detail, errors) = exception switch
        {
            // Erro de Validação de dados de entrada do FluentValidation
            FluentValidation.ValidationException valEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation Failed",
                "https://tools.ietf.org/html/rfc4918#section-11.2",
                "One or more validation errors occurred in the request payload.",
                ExtractValidationErrors(valEx)
            ),
            // Erro de Recurso Não Encontrado
            Sgae.Domain.Exceptions.NotFoundException domainNotFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                domainNotFoundEx.Message,
                null
            ),
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                notFoundEx.Message,
                null
            ),
            // Não Autorizado / Falha de Autenticação / Tokens Inválidos
            Microsoft.IdentityModel.Tokens.SecurityTokenException secTokenEx => (
                StatusCodes.Status401Unauthorized,
                "Invalid Security Token",
                "https://tools.ietf.org/html/rfc7235#section-3.1",
                secTokenEx.Message,
                null
            ),
            UnauthorizedAccessException unauthEx => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "https://tools.ietf.org/html/rfc7235#section-3.1",
                string.IsNullOrWhiteSpace(unauthEx.Message) ? "Authentication credentials are required or invalid." : unauthEx.Message,
                null
            ),
            // Erro de Regra de Negócio de Domínio
            Sgae.Domain.Exceptions.DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                domainEx.Message,
                null
            ),
            // ArgumentException ou InvalidOperationException
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Invalid Parameter",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                argEx.Message,
                null
            ),
            InvalidOperationException invOpEx => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                invOpEx.Message,
                null
            ),
            // BadHttpRequestException (ex: JSON parsing/binding inválido)
            BadHttpRequestException badReqEx => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                badReqEx.Message,
                null
            ),
            // Outros erros genéricos não tratados
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                "An unexpected server error occurred while processing the request.",
                null
            )
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "[Server Error 500] [TraceId: {TraceId}] Unhandled exception on {Method} {Path}: {ErrorMessage}",
                traceId,
                context.Request.Method,
                context.Request.Path,
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "[Request Failure {StatusCode}] [TraceId: {TraceId}] {Method} {Path} rejected: {Title} - {Detail}. Errors: {@Errors}",
                statusCode,
                traceId,
                context.Request.Method,
                context.Request.Path,
                title,
                detail,
                errors ?? (object)exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("traceId", traceId);

        if (context.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId) && !string.IsNullOrEmpty(correlationId))
        {
            problemDetails.Extensions.Add("correlationId", correlationId.ToString());
        }

        if (errors != null)
        {
            problemDetails.Extensions.Add("errors", errors);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private static Dictionary<string, string[]> ExtractValidationErrors(FluentValidation.ValidationException exception)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var error in exception.Errors)
        {
            var propertyName = string.IsNullOrEmpty(error.PropertyName) ? "general" : error.PropertyName;
            if (errors.ContainsKey(propertyName))
            {
                var list = new List<string>(errors[propertyName]) { error.ErrorMessage };
                errors[propertyName] = list.ToArray();
            }
            else
            {
                errors[propertyName] = new[] { error.ErrorMessage };
            }
        }
        return errors;
    }
}