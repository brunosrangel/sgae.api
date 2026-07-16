using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Rituais e Trabalhos Sugeridos (ex: "Ebó de Limpeza", "Amaci de Oxalá")
/// </summary>
public class RitualSugerido : BaseEntity
{
    private RitualSugerido() { }

    public RitualSugerido(string nome, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do ritual sugerido é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;

    public void Update(string nome, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do ritual sugerido é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
        RegisterUpdate();
    }
}
