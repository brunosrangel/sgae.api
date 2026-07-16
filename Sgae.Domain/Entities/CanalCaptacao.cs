using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Canais de Captação de Consulentes (ex: "Indicação de Amigos", "Instagram")
/// </summary>
public class CanalCaptacao : BaseEntity
{
    private CanalCaptacao() { }

    public CanalCaptacao(string nome, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do canal de captação é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;

    public virtual ICollection<Lead> Leads { get; private set; } = new List<Lead>();

    public void Update(string nome, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do canal de captação é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
        RegisterUpdate();
    }
}
