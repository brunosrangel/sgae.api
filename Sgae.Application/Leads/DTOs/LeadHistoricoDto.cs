using System;

namespace Sgae.Application.Leads.DTOs;

public class LeadHistoricoDto
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
