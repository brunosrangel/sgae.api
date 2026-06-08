using System;
using FluentValidation;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

public class CreateAgendamentoCommandValidator : AbstractValidator<CreateAgendamentoCommand>
{
    public CreateAgendamentoCommandValidator()
    {
        RuleFor(v => v.LeadId)
            .NotEmpty().WithMessage("O consulente associado (LeadId) é obrigatório.");

        RuleFor(v => v.DataHora)
            .NotEmpty().WithMessage("A data do agendamento é obrigatória.")
            .GreaterThan(DateTime.UtcNow).WithMessage("A data de agendamento deve ser uma data futura.");

        RuleFor(v => v.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do agendamento não pode ser negativo.");
    }
}