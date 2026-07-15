using System;
using System.Collections.Generic;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Sacerdotes Escalados (ex: "Pai Carlos de Oxóssi")
/// </summary>
public class Sacerdote : BaseEntity
{
    private Sacerdote() { }

    public Sacerdote(string nome, string? especialidade = null, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do sacerdote é obrigatório.");

        Nome = nome.Trim();
        Especialidade = especialidade?.Trim();
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public string? Especialidade { get; private set; }
    public bool Ativo { get; private set; } = true;

    public virtual ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();
    public virtual ICollection<Prescricao> Prescricoes { get; private set; } = new List<Prescricao>();
    public virtual ICollection<Conversao> Conversoes { get; private set; } = new List<Conversao>();

    public void Update(string nome, string? especialidade, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do sacerdote é obrigatório.");

        Nome = nome.Trim();
        Especialidade = especialidade?.Trim();
        Ativo = ativo;
        RegisterUpdate();
    }
}
