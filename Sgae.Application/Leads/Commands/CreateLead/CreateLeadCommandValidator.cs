using FluentValidation;

namespace Sgae.Application.Leads.Commands.CreateLead;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(v => v.Nome)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(150).WithMessage("Nome não pode exceder 150 caracteres.");

        RuleFor(v => v.Telefone)
            .NotEmpty().WithMessage("Telefone para contato é obrigatório.")
            .MaximumLength(25).WithMessage("Telefone não pode exceder 25 caracteres.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(v => v.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória.");

        RuleFor(v => v.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve conter exatamente 2 caracteres (UF).");

        RuleFor(v => v.ProblemaPrincipal)
            .NotEmpty().WithMessage("O problema principal deve ser especificado para captação.");
    }
}