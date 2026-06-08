using System;

namespace Sgae.Application.Perfis.DTOs;

public class PerfilConsulenteDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public int Idade { get; set; }
    public string FaixaEtaria { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Profissao { get; set; } = string.Empty;
    public string Escolaridade { get; set; } = string.Empty;
    public string EstadoCivil { get; set; } = string.Empty;
}
