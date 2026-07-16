using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Tabela de busca (lookup) que descreve as categorias de atendimento espiritual oferecidas no sistema.
/// </summary>
public class SpiritualAttendanceCategory : BaseEntity
{
    private SpiritualAttendanceCategory() { } // Requerido pelo EF Core

    public SpiritualAttendanceCategory(TipoAtendimento tipo, string nome, string descricao, int tempoRecomendadoMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da categoria é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da categoria é obrigatória.", nameof(descricao));
        if (tempoRecomendadoMinutos <= 0)
            throw new ArgumentException("O tempo recomendado de atendimento deve ser maior que zero.", nameof(tempoRecomendadoMinutos));

        Tipo = tipo;
        Nome = nome.Trim();
        Descricao = descricao.Trim();
        TempoRecomendadoMinutos = tempoRecomendadoMinutos;
    }

    public TipoAtendimento Tipo { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public int TempoRecomendadoMinutos { get; private set; }

    public void Update(string nome, string descricao, int tempoRecomendadoMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da categoria é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da categoria é obrigatória.", nameof(descricao));
        if (tempoRecomendadoMinutos <= 0)
            throw new ArgumentException("O tempo recomendado de atendimento deve ser maior que zero.", nameof(tempoRecomendadoMinutos));

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        TempoRecomendadoMinutos = tempoRecomendadoMinutos;
        RegisterUpdate();
    }
}
