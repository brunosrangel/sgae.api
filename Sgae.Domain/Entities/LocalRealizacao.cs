using System;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Locais de Realização de Ebós / Rituais (ex: "Cachoeira", "Mata / Floresta")
/// </summary>
public class LocalRealizacao : BaseEntity
{
    private LocalRealizacao() { }

    public LocalRealizacao(string nome, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do local de realização é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;

    public void Update(string nome, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do local de realização é obrigatório.");

        Nome = nome.Trim();
        Ativo = ativo;
        RegisterUpdate();
    }
}
