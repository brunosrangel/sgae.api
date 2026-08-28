using System;
using System.Collections.Generic;
using Sgae.Application.Perfis.DTOs;

namespace Sgae.Application.Leads.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string? CustomId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string? Profissao { get; set; }
    public string? Nacionalidade { get; set; }
    public string? Naturalidade { get; set; }

    public string? TradicaoTerreiro { get; set; }
    public string? VinculoTradicoes { get; set; }
    public string? VinculoCcrias { get; set; }
    public string? Temporalidade { get; set; }
    public string? JogouBuziosBabalorisaSidnei { get; set; }
    public List<string> OrixasNagoKetu { get; set; } = new();

    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;

    public string? Observacoes { get; set; }
    public string ProblemaPrincipal { get; set; } = string.Empty;
    public string Status { get; set; } = "Novo";
    public string Prioridade { get; set; } = "Média";
    public DateTime DataContato { get; set; }
    public DateTime DataCadastro { get; set; }
    public string Origem { get; set; } = string.Empty;

    public Guid? CanalCaptacaoId { get; set; }
    public string? CanalCaptacaoNome { get; set; }

    public List<LeadHistoricoDto> Historico { get; set; } = new();
    public PerfilConsulenteDto? Perfil { get; set; }
}
