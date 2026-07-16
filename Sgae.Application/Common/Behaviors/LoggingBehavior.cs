using MediatR;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using System.Diagnostics;

namespace Sgae.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = _correlationIdProvider.GetCorrelationId();

        _logger.LogInformation("SGAE Pipeline [CID: {CorrelationId}]: Iniciando processamento do Request {RequestName} {@Request}", correlationId, requestName, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation("SGAE Pipeline [CID: {CorrelationId}]: Finalizado processamento de {RequestName} com sucesso em {ElapsedMilliseconds}ms", correlationId, requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SGAE Pipeline [CID: {CorrelationId}]: Falha crítica na execução do Request {RequestName} após {ElapsedMilliseconds}ms", correlationId, requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}