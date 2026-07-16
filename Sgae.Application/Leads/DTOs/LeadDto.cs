using Sgae.Application.Perfis.DTOs;

namespace Sgae.Application.Leads.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string ProblemaPrincipal { get; set; } = string.Empty;
    public DateTime DataCaptacao { get; set; }
    public PerfilConsulenteDto? Perfil { get; set; }
}