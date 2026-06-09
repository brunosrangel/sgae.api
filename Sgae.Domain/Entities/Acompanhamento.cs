using System;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade de domínio rica representando o Acompanhamento de um Atendimento Espiritual.
/// Armazena o progresso, observações de melhora e novas recomendações de auxílio espiritual.
/// </summary>
public class Acompanhamento : BaseEntity
{
    private Acompanhamento() { }

    public Acompanhamento(
        Guid atendimentoEspiritualId,
        DateTime dataAcompanhamento,
        string sintomasMelhora,
        string recomendacoes,
        string observacoes)
    {
        if (atendimentoEspiritualId == Guid.Empty)
            throw new ArgumentException("O acompanhamento deve estar vinculado a um Atendimento Espiritual válido.");

        if (dataAcompanhamento > DateTime.UtcNow.AddDays(1))
            throw new ArgumentException("A data do acompanhamento não pode ser futura.");

        if (string.IsNullOrWhiteSpace(sintomasMelhora))
            throw new ArgumentException("Os sintomas de melhora ou estado geral devem ser descritos.");

        if (string.IsNullOrWhiteSpace(recomendacoes))
            throw new ArgumentException("As recomendações e orientações devem ser especificadas.");

        AtendimentoEspiritualId = atendimentoEspiritualId;
        DataAcompanhamento = dataAcompanhamento;
        SintomasMelhora = sintomasMelhora.Trim();
        Recomendacoes = recomendacoes.Trim();
        Observacoes = observacoes?.Trim() ?? string.Empty;
    }

    public Guid AtendimentoEspiritualId { get; private set; }
    public virtual AtendimentoEspiritual AtendimentoEspiritual { get; private set; } = null!;

    public DateTime DataAcompanhamento { get; private set; }
    public string SintomasMelhora { get; private set; } = null!;
    public string Recomendacoes { get; private set; } = null!;
    public string Observacoes { get; private set; } = null!;

    public void UpdateAcompanhamento(
        DateTime dataAcompanhamento,
        string sintomasMelhora,
        string recomendacoes,
        string observacoes)
    {
        if (dataAcompanhamento > DateTime.UtcNow.AddDays(1))
            throw new ArgumentException("A data do acompanhamento não pode ser futura.");

        if (string.IsNullOrWhiteSpace(sintomasMelhora))
            throw new ArgumentException("Os sintomas de melhora ou estado geral devem ser descritos.");

        if (string.IsNullOrWhiteSpace(recomendacoes))
            throw new ArgumentException("As recomendações e orientações devem ser especificadas.");

        DataAcompanhamento = dataAcompanhamento;
        SintomasMelhora = sintomasMelhora.Trim();
        Recomendacoes = recomendacoes.Trim();
        Observacoes = observacoes?.Trim() ?? string.Empty;
        RegisterUpdate();
    }
}
