using System;

namespace Sgae.Application.Agendamentos.DTOs;

public class AgendamentoDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string LeadNome { get; set; } = string.Empty;
    public string LeadTelefone { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string Modalidade { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public AgendamentoAtendimentoEspiritualDto? Atendimento { get; set; }
}