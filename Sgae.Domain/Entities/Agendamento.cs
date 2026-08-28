using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando o Agendamento de uma consulta espiritual associada a um Lead/Consulente.
/// </summary>
public class Agendamento : BaseEntity
{
    private Agendamento() { }

    public Agendamento(
        Guid leadId,
        DateTime dataHora,
        ModalidadeAtendimento modalidade,
        decimal valor)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("O agendamento deve estar associado a um consulente (LeadId) válido.");

        if (dataHora == default || dataHora == DateTime.MinValue)
            throw new ArgumentException("A data do agendamento deve ser informada e válida.");

        if (valor < 0)
            throw new ArgumentException("O valor do agendamento não pode ser negativo.");

        LeadId = leadId;
        DataHora = dataHora;
        Modalidade = modalidade;
        Valor = valor;
        Status = StatusAgendamento.Pendente;
        MotivoCancelamento = null;
    }

    public Guid LeadId { get; private set; }
    public virtual Lead Lead { get; private set; } = null!;

    public DateTime DataHora { get; private set; }
    public ModalidadeAtendimento Modalidade { get; private set; }
    public decimal Valor { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    // Etapa 4 - Relacionamento 1-para-1 com AtendimentoEspiritual
    public virtual AtendimentoEspiritual? Atendimento { get; private set; }

    public Guid? SacerdoteId { get; private set; }
    public virtual Sacerdote? Sacerdote { get; private set; }

    public Guid? ServicoConsultaId { get; private set; }
    public virtual ServicoConsulta? ServicoConsulta { get; private set; }

    // Métodos de Regras de Negócio (Status State Transitions)
    public void DefinirSacerdote(Guid? sacerdoteId)
    {
        SacerdoteId = sacerdoteId;
        RegisterUpdate();
    }

    public void DefinirServicoConsulta(Guid? servicoConsultaId)
    {
        ServicoConsultaId = servicoConsultaId;
        RegisterUpdate();
    }

    public void ConfirmarAgendamento()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new InvalidOperationException($"Não é possível confirmar um agendamento com status atual: {Status}");

        Status = StatusAgendamento.Confirmado;
        RegisterUpdate();
    }

    public void RealizarAgendamento()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new InvalidOperationException("Apenas agendamentos Confirmados podem ser marcados como Realizados.");

        Status = StatusAgendamento.Realizado;
        RegisterUpdate();
    }

    public void CancelarAgendamento(string motivo)
    {
        if (Status == StatusAgendamento.Realizado)
            throw new InvalidOperationException("Não é possível cancelar um agendamento que já foi realizado.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("O motivo do cancelamento deve ser obrigatoriamente justificado.");

        Status = StatusAgendamento.Cancelado;
        MotivoCancelamento = motivo.Trim();
        RegisterUpdate();
    }

    public void MarcarComoAusente()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new InvalidOperationException("Apenas agendamentos Confirmados podem registrar ausência (no-show).");

        Status = StatusAgendamento.Ausente;
        RegisterUpdate();
    }

    public void Reagendar(DateTime novaDataHora)
    {
        if (novaDataHora == default || novaDataHora == DateTime.MinValue)
            throw new ArgumentException("Nova data de reagendamento informada é inválida.");

        if (Status == StatusAgendamento.Realizado || Status == StatusAgendamento.Cancelado)
            throw new InvalidOperationException("Não é possível reagendar atendimentos concluídos ou cancelados.");

        DataHora = novaDataHora;
        Status = StatusAgendamento.Pendente; // Volta a requerer confirmação
        RegisterUpdate();
    }
}