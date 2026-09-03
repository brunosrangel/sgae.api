using System.ComponentModel.DataAnnotations;

namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// DTO de entrada para submissão do formulário de Atendimento e Consulta Oracular (Jogo de Búzios).
/// Totalmente compatível com o schema do frontend (Zod validation).
/// </summary>
public class AtendimentoCreateDto
{
    [Required(ErrorMessage = "O Sacerdote responsável é obrigatório.")]
    public Guid SacerdoteId { get; set; }

    [Required(ErrorMessage = "O Consulente é obrigatório.")]
    public Guid ConsulenteId { get; set; }

    public Guid? AgendamentoId { get; set; }

    [Required(ErrorMessage = "A Data e Horário da consulta são obrigatórios.")]
    public DateTime DataConsulta { get; set; }

    [Required(ErrorMessage = "O Tipo de Oráculo é obrigatório.")]
    public string TipoOraculo { get; set; } = "Jogo de Búzios";

    [Required(ErrorMessage = "A Pergunta Central / Dúvida Vital é obrigatória.")]
    [MinLength(5, ErrorMessage = "A Pergunta Central deve ter no mínimo 5 caracteres.")]
    public string PerguntaCentral { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Veredicto Espiritual / Causa Raiz é obrigatório.")]
    [MinLength(10, ErrorMessage = "O Veredicto Espiritual deve ter no mínimo 10 caracteres.")]
    public string VeredictoEspiritual { get; set; } = string.Empty;

    public string? Status { get; set; } = "Realizado";

    public string? Observacoes { get; set; }

    public List<AnexoAtendimentoCreateDto> Anexos { get; set; } = new();
}
