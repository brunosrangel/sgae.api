using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sgae.API.Middlewares;

/// <summary>
/// Middleware global para capturar e normalizar erros (RFC 7807) antes de responder ao cliente.
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
            _logger.LogError(ex, "Ocorreu uma exceção não tratada na requisição no SGAE.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, errors) = exception switch
        {
            // Erro de Domínio - Invariante violada (ex: agendamento retroativo)
            Sgae.Domain.Exceptions.DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                domainEx.Message,
                null
            ),
            // Erro de Validação de dados de entrada do FluentValidation (etapas de entrada)
            FluentValidation.ValidationException valEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation Failed",
                "Um ou mais erros de validação ocorreram na entrada dos dados.",
                ExtractValidationErrors(valEx)
            ),
            // Exceção de Argumentos inválidos
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Invalid Parameter",
                argEx.Message,
                null
            ),
            // Outros erros genéricos inexplicados
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Ocorreu um erro interno inesperado nos servidores do SGAE.",
                null
            )
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

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
            if (errors.ContainsKey(error.PropertyName))
            {
                var list = new List<string>(errors[error.PropertyName]) { error.ErrorMessage };
                errors[error.PropertyName] = list.ToArray();
            }
            else
            {
                errors[error.PropertyName] = new[] { error.ErrorMessage };
            }
        }
        return errors;
    }
}