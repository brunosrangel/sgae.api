using MediatR;
using Sgae.Domain.Common;

namespace Sgae.Application.Common.Events;

/// <summary>
/// Wrapper para envelopar qualquer IDomainEvent do domínio em uma INotification compatível com MediatR.
/// Permite o desacoplamento absoluto da camada de Domínio em relação ao pipeline de aplicação.
/// </summary>
/// <typeparam name="TDomainEvent">O tipo concreto do evento de domínio.</typeparam>
public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
