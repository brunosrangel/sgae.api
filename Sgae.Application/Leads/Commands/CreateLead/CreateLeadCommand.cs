using System;
using System.Collections.Generic;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Leads.DTOs;
using Sgae.Domain.Enums;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Comando contendo os parâmetros de entrada para cadastro de um novo Consulente (Lead).
/// Totalmente compatível com o JSON de cadastro de Leads.
/// </summary>
public class CreateLeadCommand : ICommand<Guid>
{
    public string? Id { get; set; }
    public string? Nome { get; set; }
    public string? NomeCompleto { get; set; }
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
    public List<string>? OrixasNagoKetu { get; set; } = new();

    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Uf { get; set; }

    public OrigemContato? Origem { get; set; }
    public string? ProblemaPrincipal { get; set; }
    public string? Observacoes { get; set; }
    public string? Status { get; set; }
    public string? Prioridade { get; set; }
    public DateTime? DataContato { get; set; }
    public DateTime? DataCadastro { get; set; }
    public Guid? CanalCaptacaoId { get; set; }

    public List<LeadHistoricoInputDto>? Historico { get; set; } = new();

    public string GetNomeEfetivo() => !string.IsNullOrWhiteSpace(NomeCompleto) ? NomeCompleto : (Nome ?? string.Empty);
    public string GetCidadeEfetiva() => !string.IsNullOrWhiteSpace(Cidade) ? Cidade : "São Paulo";
    public string GetEstadoEfetivo() => !string.IsNullOrWhiteSpace(Uf) ? Uf : (!string.IsNullOrWhiteSpace(Estado) ? Estado : "SP");
    public string GetProblemaPrincipalEfetivo() => !string.IsNullOrWhiteSpace(Observacoes) ? Observacoes : (!string.IsNullOrWhiteSpace(ProblemaPrincipal) ? ProblemaPrincipal : "Captação de Lead");
}
