using Sgae.Application.Abstractions;

namespace Sgae.Application.Common;

/// <summary>
/// Implementação escopada responsável por reter o CorrelationId para acesso unificado nas camadas de negócio.
/// </summary>
public class CorrelationIdProvider : ICorrelationIdProvider
{
    private Guid _correlationId = Guid.NewGuid();

    public Guid GetCorrelationId() => _correlationId;

    public void SetCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
    }
}
