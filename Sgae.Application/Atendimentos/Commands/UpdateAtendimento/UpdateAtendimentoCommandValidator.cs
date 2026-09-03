using FluentValidation;

namespace Sgae.Application.Atendimentos.Commands.UpdateAtendimento;

/// <summary>
/// Validador fluente para o comando de atualização de Atendimento Espiritual.
/// </summary>
public class UpdateAtendimentoCommandValidator : AbstractValidator<UpdateAtendimentoCommand>
{
    public UpdateAtendimentoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador único (ID) do Atendimento Espiritual é obrigatório.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("O tipo de atendimento espiritual fornecido é inválido.");

        RuleFor(x => x.TempoDuracaoMinutos)
            .GreaterThan(0).WithMessage("O tempo de duração em minutos deve ser maior que zero.")
            .LessThanOrEqualTo(480).WithMessage("O tempo de duração não pode exceder 480 minutos (8 horas).");

        RuleFor(x => x.TemasAbordados)
            .NotEmpty().WithMessage("É obrigatório especificar os temas abordados no atendimento espiritual.")
            .MaximumLength(1000).WithMessage("Os temas abordados podem ter no máximo 1000 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("As observações adicionais podem ter no máximo 2000 caracteres.");
    }
}
