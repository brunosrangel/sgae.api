using System;
using FluentValidation;
using Sgae.Domain.Common;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

public class CreateAgendamentoCommandValidator : AbstractValidator<CreateAgendamentoCommand>
{
    public CreateAgendamentoCommandValidator()
    {
        RuleFor(v => v.LeadId)
            .NotEmpty().WithMessage("O consulente associado (LeadId) é obrigatório.");

        // Se 'Data' for fornecido separadamente pelo front-end, valida se está em formato ISO 8601 válido (ex: yyyy-MM-dd)
        RuleFor(v => v.Data)
            .Must(data => string.IsNullOrWhiteSpace(data) || DateTimeHelper.IsValidIso8601(data))
            .WithMessage("A data informada deve seguir um formato ISO 8601 válido (ex: yyyy-MM-dd).");

        // Valida se a data e hora combinadas final não são nulas/vazias e não estão no passado
        RuleFor(v => v.DataHora)
            .NotEmpty().WithMessage("A data e hora do agendamento são obrigatórias.")
            .Must(dt => dt > DateTime.UtcNow)
            .WithMessage("A data de agendamento deve ser uma data futura.");

        RuleFor(v => v.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do agendamento não pode ser negativo.");
    }
}
