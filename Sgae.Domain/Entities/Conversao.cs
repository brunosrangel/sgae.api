using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Conversões acompanhadas pelo Sacerdote
/// </summary>
public class Conversao : BaseEntity
{
    private Conversao() { }

    public Conversao(Guid sacerdoteId, string descricao)
    {
        if (sacerdoteId == Guid.Empty)
            throw new ArgumentException("A conversão deve estar vinculada a um Sacerdote válido.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da conversão é obrigatória.");

        SacerdoteId = sacerdoteId;
        Descricao = descricao.Trim();
    }

    public Guid SacerdoteId { get; private set; }
    public virtual Sacerdote Sacerdote { get; private set; } = null!;

    public string Descricao { get; private set; } = null!;

    public void Update(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da conversão é obrigatória.");

        Descricao = descricao.Trim();
        RegisterUpdate();
    }
}
