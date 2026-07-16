using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Events;

/// <summary>
/// Evento de domínio disparado sempre que um novo Atendimento Espiritual (Etapa 4) é registrado com sucesso.
/// </summary>
public class AtendimentoEspiritualCriadoEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid AtendimentoId { get; }
    public Guid AgendamentoId { get; }
    public TipoAtendimento Tipo { get; }
    public int TempoDuracaoMinutos { get; }
    public string TemasAbordados { get; }

    public AtendimentoEspiritualCriadoEvent(
        Guid atendimentoId,
        Guid agendamentoId,
        TipoAtendimento tipo,
        int tempoDuracaoMinutos,
        string temasAbordados)
    {
        AtendimentoId = atendimentoId;
        AgendamentoId = agendamentoId;
        Tipo = tipo;
        TempoDuracaoMinutos = tempoDuracaoMinutos;
        TemasAbordados = temasAbordados;
    }
}
