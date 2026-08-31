using System;

namespace Sgae.Application.Agendamentos.DTOs;

public class AgendamentoDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string LeadNome { get; set; } = string.Empty;
    public string LeadTelefone { get; set; } = string.Empty;
    public string? Data { get; set; }
    public string? Horario { get; set; }
    public DateTime DataHora { get; set; }
    public string Modalidade { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    public Guid? SacerdoteId { get; set; }
    public string? Sacerdote { get; set; }
    public string? SacerdoteNome { get; set; }

    public Guid? ServicoConsultaId { get; set; }
    public string? TipoConsulta { get; set; }
    public string? ServicoConsultaNome { get; set; }

    public string? FormaPagamento { get; set; }
    public bool Pago { get; set; }
    public string? Observacoes { get; set; }
    public bool WhatsappConfirmacaoDisparada { get; set; }
    public string? ConfigLembrete { get; set; }
    public string? FrequenciaLembrete { get; set; }

    public AgendamentoAtendimentoEspiritualDto? Atendimento { get; set; }
}