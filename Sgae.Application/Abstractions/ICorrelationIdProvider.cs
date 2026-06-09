using System;

namespace Sgae.Application.Abstractions;

/// <summary>
/// Provedor para obtenção e controle do Correlation ID associado ao escopo de execução atual (Request).
/// </summary>
public interface ICorrelationIdProvider
{
    Guid GetCorrelationId();
    void SetCorrelationId(Guid correlationId);
}
