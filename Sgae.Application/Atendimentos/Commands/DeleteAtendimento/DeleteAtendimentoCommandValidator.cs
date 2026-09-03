using FluentValidation;

namespace Sgae.Application.Atendimentos.Commands.DeleteAtendimento;

/// <summary>
/// Validador fluente para o comando de exclusão lógica de Atendimento Espiritual.
/// </summary>
public class DeleteAtendimentoCommandValidator : AbstractValidator<DeleteAtendimentoCommand>
{
    public DeleteAtendimentoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador único (ID) do Atendimento Espiritual é obrigatório.");
    }
}
