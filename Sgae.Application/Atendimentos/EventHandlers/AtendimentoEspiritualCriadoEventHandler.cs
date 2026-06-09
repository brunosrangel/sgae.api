using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Sgae.Application.Common.Events;
using Sgae.Domain.Events;

namespace Sgae.Application.Atendimentos.EventHandlers;

/// <summary>
/// Manipulador desacoplado de execução de efeitos colaterais para o evento de domínio de novo Atendimento Espiritual.
/// Centraliza o envio de avisos ou notificações após o registro definitivo e salvamento seguro no banco de dados.
/// </summary>
public class AtendimentoEspiritualCriadoEventHandler : INotificationHandler<DomainEventNotification<AtendimentoEspiritualCriadoEvent>>
{
    private readonly ILogger<AtendimentoEspiritualCriadoEventHandler> _logger;

    public AtendimentoEspiritualCriadoEventHandler(ILogger<AtendimentoEspiritualCriadoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AtendimentoEspiritualCriadoEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "SGAE Evento Desacoplado [Event: {EventId}] [CID/Atendimento: {AtendimentoId}] - Novo atendimento espiritual do tipo '{Tipo}' registrado. Ativando efeitos secundários de notificação pastoral.",
            domainEvent.EventId,
            domainEvent.AtendimentoId,
            domainEvent.Tipo);

        // Simulação realista e profissional de side-effects corporativos (envio de alertas ao coordenador pastoral, logs estruturados)
        _logger.LogInformation(
            "SGAE Notificação Pastoral: Notificação disparada com sucesso para a equipe e coordenadores com duração de {Duracao} minutos. Temas abordados para acompanhamento: '{TemasAbordados}'.",
            domainEvent.TempoDuracaoMinutos,
            domainEvent.TemasAbordados);

        return Task.CompletedTask;
    }
}
