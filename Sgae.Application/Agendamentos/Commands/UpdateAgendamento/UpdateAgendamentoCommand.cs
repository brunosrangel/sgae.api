using System;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Commands.UpdateAgendamento;

/// <summary>
/// Comando para atualizar os dados de um agendamento existente.
/// </summary>
public class UpdateAgendamentoCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadNome { get; set; }
    public string? LeadTelefone { get; set; }
    public string? Data { get; set; }
    public string? Horario { get; set; }
    public DateTime? DataHora { get; set; }
    public ModalidadeAtendimento? Modalidade { get; set; }
    public decimal? Valor { get; set; }
    public string? Status { get; set; }
    public Guid? SacerdoteId { get; set; }
    public string? Sacerdote { get; set; }
    public Guid? ServicoConsultaId { get; set; }
    public string? TipoConsulta { get; set; }
    public string? FormaPagamento { get; set; }
    public bool? Pago { get; set; }
    public string? Observacoes { get; set; }
    public bool? WhatsappConfirmacaoDisparada { get; set; }
    public string? ConfigLembrete { get; set; }
    public string? FrequenciaLembrete { get; set; }
    public string? MotivoCancelamento { get; set; }

    public UpdateAgendamentoCommand() { }

    public UpdateAgendamentoCommand(Guid id)
    {
        Id = id;
    }
}
