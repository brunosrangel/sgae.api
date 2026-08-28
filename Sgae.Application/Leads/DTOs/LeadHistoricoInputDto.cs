using System;

namespace Sgae.Application.Leads.DTOs;

public class LeadHistoricoInputDto
{
    public DateTime? Data { get; set; }
    public string Tipo { get; set; } = "Criação";
    public string Descricao { get; set; } = string.Empty;
}
