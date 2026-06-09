using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Sgae.Application.Abstractions;

namespace Sgae.API.Middlewares;

/// <summary>
/// Middleware corporativo para assegurar que todas as requisições possuam um Correlation ID (ID de Correlação).
/// O ID é capturado ou gerado, retornado nos cabeçalhos HTTP e injetado no LogContext do Serilog para rastreabilidade end-to-end.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderKey = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        // 1. Tentar ler o Correlation ID existente do cabeçalho da requisição ou gerar um novo de forma segura
        var correlationIdHeader = context.Request.Headers[CorrelationIdHeaderKey].FirstOrDefault();
        
        Guid correlationId = Guid.TryParse(correlationIdHeader, out var parsedGuid) 
            ? parsedGuid 
            : Guid.NewGuid();

        // 2. Definir o Correlation ID no provedor escopado das camadas internas (CQRS Pipeline)
        correlationIdProvider.SetCorrelationId(correlationId);

        // 3. Acrescentar o Correlation ID no cabeçalho de resposta HTTP para fins de auditoria do cliente / API Gateway
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
            {
                context.Response.Headers[CorrelationIdHeaderKey] = correlationId.ToString();
            }
            return Task.CompletedTask;
        });

        // 4. Enriquecer dinamicamente o LogContext do Serilog para que o Correlation ID apareça em todos os logs das camadas
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await _next(context);
        }
    }
}
