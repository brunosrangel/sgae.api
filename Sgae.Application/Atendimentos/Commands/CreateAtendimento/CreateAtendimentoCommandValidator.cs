using FluentValidation;

namespace Sgae.Application.Atendimentos.Commands.CreateAtendimento;

/// <summary>
/// Validador fluente para o comando de registro de Atendimento Espiritual.
/// </summary>
public class CreateAtendimentoCommandValidator : AbstractValidator<CreateAtendimentoCommand>
{
    public CreateAtendimentoCommandValidator()
    {
        RuleFor(x => x.AgendamentoId)
            .NotEmpty().WithMessage("O ID do Agendamento associado é obrigatório.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("O tipo de atendimento fornecido é inválido.");

        RuleFor(x => x.TempoDuracaoMinutos)
            .GreaterThan(0).WithMessage("O tempo de duração em minutos deve ser maior que zero.");

        RuleFor(x => x.TemasAbordados)
            .NotEmpty().WithMessage("É obrigatório especificar os temas abordados no atendimento espiritual.")
            .MaximumLength(1000).WithMessage("Os temas abordados podem ter no máximo 1000 caracteres.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000).WithMessage("As observações adicionais podem ter no máximo 2000 caracteres.");
    }
}
