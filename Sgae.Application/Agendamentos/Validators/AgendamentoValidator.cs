using System;
using FluentValidation;
using Sgae.Domain.Entities;

namespace Sgae.Application.Agendamentos.Validators;

public class AgendamentoValidator : AbstractValidator<Agendamento>
{
    public AgendamentoValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("O consulente associado (LeadId) é obrigatório.");

        RuleFor(x => x.DataHora)
            .NotEmpty().WithMessage("A data do agendamento é obrigatória.")
            .Must(data => data > DateTime.UtcNow).WithMessage("A data e hora do agendamento devem ser futuras.");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do agendamento não pode ser negativo.");
    }
}
