using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Representa um perfil ou cargo pastoral configurado no sistema para controle robusto de autorização.
/// </summary>
public class PastoralRole : BaseEntity
{
    private PastoralRole() { } // Requerido pelo EF Core

    public PastoralRole(string nome, string descricao, string escopoPermissao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do cargo pastoral é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do cargo pastoral é obrigatória.", nameof(descricao));
        if (string.IsNullOrWhiteSpace(escopoPermissao))
            throw new ArgumentException("O escopo de permissão do cargo é obrigatório.", nameof(escopoPermissao));

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        EscopoPermissao = escopoPermissao.Trim();
    }

    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public string EscopoPermissao { get; private set; } = null!;

    public void Update(string descricao, string escopoPermissao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do cargo pastoral é obrigatória.", nameof(descricao));
        if (string.IsNullOrWhiteSpace(escopoPermissao))
            throw new ArgumentException("O escopo de permissão do cargo é obrigatório.", nameof(escopoPermissao));

        Descricao = descricao.Trim();
        EscopoPermissao = escopoPermissao.Trim();
        RegisterUpdate();
    }
}
