using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Categorias de Despesas Administrativas / Litúrgicas (ex: "Manutenção", "Administrativo")
/// </summary>
public class CategoriaDespesa : BaseEntity
{
    private CategoriaDespesa() { }

    public CategoriaDespesa(string nome, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da categoria de despesa é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;

    public virtual ICollection<CustoInsumo> Custos { get; private set; } = new List<CustoInsumo>();

    public void Update(string nome, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da categoria de despesa é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
        RegisterUpdate();
    }
}
