using Sgae.Application.Common.CQRS;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Comando contendo as especificações do agendamento da consulta espiritual.
/// Suporta recebimento de data/hora unificados ou separados em data e horário.
/// </summary>
public class CreateAgendamentoCommand : ICommand<Guid>
{
    private DateTime? _dataHora;

    public Guid LeadId { get; set; }

    /// <summary>
    /// Data e Hora do agendamento. Se não for enviada diretamente,
    /// é inferida através da combinação das propriedades Data e Horario.
    /// </summary>
    public DateTime DataHora
    {
        get
        {
            if (_dataHora.HasValue && _dataHora.Value != default)
            {
                return _dataHora.Value;
            }

            if (!string.IsNullOrWhiteSpace(Data))
            {
                try
                {
                    return DateTimeHelper.ParseUtc(Data, Horario);
                }
                catch
                {
                    // Silencioso, retorna o fallback default
                }
            }

            return _dataHora ?? default;
        }
        set => _dataHora = value;
    }

    /// <summary>
    /// Data enviada separadamente pelo front-end (ex: "2026-07-21")
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Horário enviado separadamente pelo front-end (ex: "08:00")
    /// </summary>
    public string? Horario { get; set; }

    /// <summary>
    /// Modalidade do atendimento (Presencial por padrão, se não informada)
    /// </summary>
    public ModalidadeAtendimento Modalidade { get; set; } = ModalidadeAtendimento.Presencial;

    public decimal Valor { get; set; }

    // Propriedades adicionais enviadas pelo JSON do front-end para perfeita compatibilidade de payload
    public Guid? SacerdoteId { get; set; }
    public string? Sacerdote { get; set; }
    public Guid? ServicoConsultaId { get; set; }
    public string? TipoConsulta { get; set; }
    public string? Status { get; set; }
    public string? FormaPagamento { get; set; }
    public bool Pago { get; set; }
    public string? Observacoes { get; set; }
    public bool WhatsappConfirmacaoDisparada { get; set; }
    public string? ConfigLembrete { get; set; }
    public string? FrequenciaLembrete { get; set; }
    public string? LeadNome { get; set; }
    public string? LeadTelefone { get; set; }
    public object? Atendimento { get; set; }

    // Construtores para compatibilidade
    public CreateAgendamentoCommand()
    {
    }

    public CreateAgendamentoCommand(Guid leadId, DateTime dataHora, ModalidadeAtendimento modalidade, decimal valor)
    {
        LeadId = leadId;
        _dataHora = dataHora;
        Modalidade = modalidade;
        Valor = valor;
    }
}
