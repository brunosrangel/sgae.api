using System;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Configuração geral de ambiente e regras de negócios persistida de forma dinâmica e escalável para o SGAE.
/// </summary>
public class SystemConfiguration : BaseEntity
{
    private SystemConfiguration() { } // Requerido pelo EF Core

    public SystemConfiguration(string chave, string valor, string descricao)
    {
        if (string.IsNullOrWhiteSpace(chave)) 
            throw new ArgumentException("A chave de configuração é obrigatória.", nameof(chave));
        if (string.IsNullOrWhiteSpace(valor)) 
            throw new ArgumentException("O valor de configuração é obrigatório.", nameof(valor));
        if (string.IsNullOrWhiteSpace(descricao)) 
            throw new ArgumentException("A descrição de configuração é obrigatória.", nameof(descricao));

        Chave = chave.Trim();
        Valor = valor.Trim();
        Descricao = descricao.Trim();
    }

    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;

    public void UpdateValue(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) 
            throw new ArgumentException("O valor de configuração é obrigatório.", nameof(valor));

        Valor = valor.Trim();
        RegisterUpdate();
    }
}
