using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando a Etapa 3 - Perfil do Consulente contendo dados demográficos adicionais.
/// </summary>
public class PerfilConsulente : BaseEntity
{
    private PerfilConsulente() { }

    public PerfilConsulente(
        Guid leadId,
        int idade,
        string faixaEtaria,
        string genero,
        string profissao,
        string escolaridade,
        string estadoCivil)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("O perfil deve estar vinculado a um Consulente (LeadId) válido.");

        if (idade <= 0 || idade > 120)
            throw new ArgumentException("A idade informada deve ser válida (entre 1 e 120 anos).");

        LeadId = leadId;
        UpdatePerfil(idade, faixaEtaria, genero, profissao, escolaridade, estadoCivil);
    }

    public Guid LeadId { get; private set; }
    public virtual Lead Lead { get; private set; } = null!;

    public int Idade { get; private set; }
    public string FaixaEtaria { get; private set; } = null!;
    public string Genero { get; private set; } = null!;
    public string Profissao { get; private set; } = null!;
    public string Escolaridade { get; private set; } = null!;
    public string EstadoCivil { get; private set; } = null!;

    public void UpdatePerfil(
        int idade,
        string faixaEtaria,
        string genero,
        string profissao,
        string escolaridade,
        string estadoCivil)
    {
        if (idade <= 0 || idade > 120)
            throw new ArgumentException("Idade deve ser entre 1 e 120 anos.");

        if (string.IsNullOrWhiteSpace(faixaEtaria))
            throw new ArgumentException("Faixa etária é obrigatória.");

        if (string.IsNullOrWhiteSpace(genero))
            throw new ArgumentException("Gênero é obrigatório.");

        if (string.IsNullOrWhiteSpace(profissao))
            throw new ArgumentException("Profissão é obrigatória.");

        if (string.IsNullOrWhiteSpace(escolaridade))
            throw new ArgumentException("Escolaridade é obrigatória.");

        if (string.IsNullOrWhiteSpace(estadoCivil))
            throw new ArgumentException("Estado civil é obrigatório.");

        Idade = idade;
        FaixaEtaria = faixaEtaria.Trim();
        Genero = genero.Trim();
        Profissao = profissao.Trim();
        Escolaridade = escolaridade.Trim();
        EstadoCivil = estadoCivil.Trim();
        RegisterUpdate();
    }
}