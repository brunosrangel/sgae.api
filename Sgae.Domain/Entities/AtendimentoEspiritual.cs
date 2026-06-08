using System;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade de domínio rica representando a Etapa 4 - Atendimento Espiritual.
/// Armazena os detalhes da consulta realizada, incluindo tipo, tempo de duração e temas abordados.
/// </summary>
public class AtendimentoEspiritual : BaseEntity
{
    private AtendimentoEspiritual() { }

    public AtendimentoEspiritual(
        Guid agendamentoId,
        TipoAtendimento tipo,
        int tempoDuracaoMinutos,
        string temasAbordados,
        string observacoes)
    {
        if (agendamentoId == Guid.Empty)
            throw new ArgumentException("O atendimento deve estar vinculado a um Agendamento válido.");

        if (tempoDuracaoMinutos <= 0)
            throw new ArgumentException("O tempo de duração do atendimento deve ser positivo.");

        if (string.IsNullOrWhiteSpace(temasAbordados))
            throw new ArgumentException("Os temas abordados devem ser obrigatoriamente especificados.");

        AgendamentoId = agendamentoId;
        Tipo = tipo;
        TempoDuracaoMinutos = tempoDuracaoMinutos;
        TemasAbordados = temasAbordados.Trim();
        Observacoes = observacoes?.Trim() ?? string.Empty;
    }

    public Guid AgendamentoId { get; private set; }
    public virtual Agendamento Agendamento { get; private set; } = null!;

    public TipoAtendimento Tipo { get; private set; }
    public int TempoDuracaoMinutos { get; private set; }
    public string TemasAbordados { get; private set; } = null!;
    public string Observacoes { get; private set; } = null!;

    public void UpdateAtendimento(
        TipoAtendimento tipo,
        int tempoDuracaoMinutos,
        string temasAbordados,
        string observacoes)
    {
        if (tempoDuracaoMinutos <= 0)
            throw new ArgumentException("O tempo de duração do atendimento deve ser positivo.");

        if (string.IsNullOrWhiteSpace(temasAbordados))
            throw new ArgumentException("Os temas abordados devem ser obrigatoriamente especificados.");

        Tipo = tipo;
        TempoDuracaoMinutos = tempoDuracaoMinutos;
        TemasAbordados = temasAbordados.Trim();
        Observacoes = observacoes?.Trim() ?? string.Empty;
        RegisterUpdate();
    }
}
