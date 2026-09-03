using FluentValidation;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Validators;

/// <summary>
/// Validador fluente para o DTO de Atendimento Espiritual para assegurar a integridade dos dados na camada de API.
/// </summary>
public class AtendimentoEspiritualDtoValidator : AbstractValidator<AtendimentoEspiritualDto>
{
    public AtendimentoEspiritualDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do atendimento é obrigatório.");

        RuleFor(x => x.AgendamentoId)
            .NotEmpty().WithMessage("O ID do agendamento associado é obrigatório.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("O tipo de atendimento fornecido é inválido.");

        RuleFor(x => x.TempoDuracaoMinutos)
            .GreaterThan(0).WithMessage("A duração da consulta deve ser maior que zero.")
            .LessThanOrEqualTo(480).WithMessage("A duração não pode exceder 480 minutos.");

        RuleFor(x => x.TemasAbordados)
            .NotEmpty().WithMessage("Os temas abordados são obrigatórios.")
            .MaximumLength(1000).WithMessage("Os temas abordados não podem exceder 1000 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("As observações adicionais podem ter no máximo 2000 caracteres.");
    }
}

/// <summary>
/// Validador fluente para o DTO de Acompanhamento Espiritual.
/// </summary>
public class AcompanhamentoDtoValidator : AbstractValidator<AcompanhamentoDto>
{
    public AcompanhamentoDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do acompanhamento é obrigatório.");

        RuleFor(x => x.AtendimentoEspiritualId)
            .NotEmpty().WithMessage("O ID do atendimento espiritual associado é obrigatório.");

        RuleFor(x => x.SintomasMelhora)
            .NotEmpty().WithMessage("O relato de sintomas e melhora é obrigatório.")
            .MaximumLength(1000).WithMessage("O relato não pode exceder 1000 caracteres.");

        RuleFor(x => x.Recomendacoes)
            .NotEmpty().WithMessage("As recomendações espirituais são obrigatórias.")
            .MaximumLength(1000).WithMessage("As recomendações não podem exceder 1000 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("As observações adicionais podem ter no máximo 2000 caracteres.");
    }
}
