using System;

namespace Sgae.Domain.Common;

/// <summary>
/// Contrato base marcador para todos os eventos de domínio disparados pela camada de negócio.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
