using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Prescrições feitas pelo Sacerdote
/// </summary>
public class Prescricao : BaseEntity
{
    private Prescricao() { }

    public Prescricao(Guid sacerdoteId, string descricao)
    {
        if (sacerdoteId == Guid.Empty)
            throw new ArgumentException("A prescrição deve estar vinculada a um Sacerdote válido.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da prescrição é obrigatória.");

        SacerdoteId = sacerdoteId;
        Descricao = descricao.Trim();
    }

    public Guid SacerdoteId { get; private set; }
    public virtual Sacerdote Sacerdote { get; private set; } = null!;

    public string Descricao { get; private set; } = null!;

    public void Update(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da prescrição é obrigatória.");

        Descricao = descricao.Trim();
        RegisterUpdate();
    }
}
