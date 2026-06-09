using System;
using FluentValidation;

namespace Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;

/// <summary>
/// Validador fluente para o comando de registro de Acompanhamento.
/// </summary>
public class CreateAcompanhamentoCommandValidator : AbstractValidator<CreateAcompanhamentoCommand>
{
    public CreateAcompanhamentoCommandValidator()
    {
        RuleFor(x => x.AtendimentoEspiritualId)
            .NotEmpty().WithMessage("O ID do Atendimento Espiritual associado é obrigatório.");

        RuleFor(x => x.DataAcompanhamento)
            .Must(data => data <= DateTime.UtcNow.AddDays(1)).WithMessage("A data do acompanhamento não pode ser futura.");

        RuleFor(x => x.SintomasMelhora)
            .NotEmpty().WithMessage("Os sintomas de melhora ou estado geral do consulente devem ser detalhados.")
            .MaximumLength(1000).WithMessage("Os sintomas de melhora podem ter no máximo 1000 caracteres.");

        RuleFor(x => x.Recomendacoes)
            .NotEmpty().WithMessage("As novas orientações/recomendações espirituais são obrigatórias.")
            .MaximumLength(1000).WithMessage("As recomendações podem ter no máximo 1000 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("As observações adicionais podem ter no máximo 2000 caracteres.");
    }
}
