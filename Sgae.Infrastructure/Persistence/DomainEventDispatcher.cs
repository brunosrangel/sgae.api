using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using Sgae.Domain.Common;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Provedor infraestrutural para capturar, limpar e distribuir os eventos de domínio registrados no ciclo transacional.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly AppDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        AppDbContext context, 
        IPublisher publisher, 
        ILogger<DomainEventDispatcher> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        // 1. Localizar todas as entidades que possuem eventos pendentes no Change Tracker do EF Core
        var domainEntities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        if (!domainEvents.Any())
        {
            return;
        }

        _logger.LogInformation(
            "SGAE Event Dispatcher: Identificados {Count} evento(s) de domínio pendente(s) para correspondência.", 
            domainEvents.Count);

        // 2. Limpar os eventos de domínio das entidades antes do despacho para evitar loops ou re-entrada acidental
        foreach (var entity in domainEntities)
        {
            entity.Entity.ClearDomainEvents();
        }

        // 3. Publicar de forma desacoplada cada evento envelopado no DomainEventNotification genérico
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            _logger.LogInformation(
                "SGAE Event Dispatcher: Envelopando evento {EventName} [ID: {EventId}] como INotification para publicação.", 
                eventType.Name, 
                domainEvent.EventId);

            var notificationType = typeof(Sgae.Application.Common.Events.DomainEventNotification<>)
                .MakeGenericType(eventType);

            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification is INotification mediatrNotification)
            {
                await _publisher.Publish(mediatrNotification, cancellationToken);
            }
        }
    }
}
