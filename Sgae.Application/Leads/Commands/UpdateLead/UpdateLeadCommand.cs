using System;
using System.Collections.Generic;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Leads.Commands.UpdateLead;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para atualizar um Consulente (Lead).
/// </summary>
public class UpdateLeadCommand : ICommand
{
    public UpdateLeadCommand() { }

    public UpdateLeadCommand(
        Guid id,
        string nome,
        string telefone,
        string email,
        string cidade,
        string estado,
        string? problemaPrincipal = null)
    {
        Id = id;
        Nome = nome;
        NomeCompleto = nome;
        Telefone = telefone;
        Email = email;
        Cidade = cidade;
        Estado = estado;
        Uf = estado;
        ProblemaPrincipal = problemaPrincipal;
    }

    public Guid Id { get; set; }
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
    public List<string>? OrixasNagoKetu { get; set; }

    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Uf { get; set; }

    public string? ProblemaPrincipal { get; set; }
    public string? Observacoes { get; set; }
    public string? Status { get; set; }
    public string? Prioridade { get; set; }
    public Guid? CanalCaptacaoId { get; set; }

    public string GetNomeEfetivo() => !string.IsNullOrWhiteSpace(NomeCompleto) ? NomeCompleto : (Nome ?? string.Empty);
    public string GetCidadeEfetiva() => !string.IsNullOrWhiteSpace(Cidade) ? Cidade : "São Paulo";
    public string GetEstadoEfetivo() => !string.IsNullOrWhiteSpace(Uf) ? Uf : (!string.IsNullOrWhiteSpace(Estado) ? Estado : "SP");
}
