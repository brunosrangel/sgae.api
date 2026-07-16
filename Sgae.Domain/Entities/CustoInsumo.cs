using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Custos de Insumos e Despesas do Terreiro
/// </summary>
public class CustoInsumo : BaseEntity
{
    private CustoInsumo() { }

    public CustoInsumo(string descricao, decimal valor, Guid? categoriaInsumoId = null, Guid? categoriaDespesaId = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do custo é obrigatória.");

        if (valor < 0)
            throw new ArgumentException("O valor do custo não pode ser negativo.");

        Descricao = descricao.Trim();
        Valor = valor;
        CategoriaInsumoId = categoriaInsumoId;
        CategoriaDespesaId = categoriaDespesaId;
    }

    public string Descricao { get; private set; } = null!;
    public decimal Valor { get; private set; }

    public Guid? CategoriaInsumoId { get; private set; }
    public virtual CategoriaInsumo? CategoriaInsumo { get; private set; }

    public Guid? CategoriaDespesaId { get; private set; }
    public virtual CategoriaDespesa? CategoriaDespesa { get; private set; }

    public void Update(string descricao, decimal valor, Guid? categoriaInsumoId, Guid? categoriaDespesaId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do custo é obrigatória.");

        if (valor < 0)
            throw new ArgumentException("O valor do custo não pode ser negativo.");

        Descricao = descricao.Trim();
        Valor = valor;
        CategoriaInsumoId = categoriaInsumoId;
        CategoriaDespesaId = categoriaDespesaId;
        RegisterUpdate();
    }
}
