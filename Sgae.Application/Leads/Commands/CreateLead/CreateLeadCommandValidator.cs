using FluentValidation;

namespace Sgae.Application.Leads.Commands.CreateLead;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(v => v.GetNomeEfetivo())
            .NotEmpty().WithMessage("Nome/NomeCompleto do consulente é obrigatório.")
            .MaximumLength(150).WithMessage("Nome não pode exceder 150 caracteres.");

        RuleFor(v => v.Telefone)
            .NotEmpty().WithMessage("Telefone para contato é obrigatório.")
            .MaximumLength(25).WithMessage("Telefone não pode exceder 25 caracteres.")
            .Matches(@"^\+?[0-9\(\)\s\-]{8,25}$").WithMessage("Telefone em formato inválido. Use caracteres numéricos, parênteses, espaço ou hífens (ex: 11987654321 ou (11) 99999-9999).");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .MaximumLength(100).WithMessage("Email não pode exceder 100 caracteres.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(v => v.GetCidadeEfetiva())
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.");

        RuleFor(v => v.GetEstadoEfetivo())
            .NotEmpty().WithMessage("Estado (UF) é obrigatório.")
            .Length(2).WithMessage("Estado (UF) deve conter exatamente 2 caracteres (ex: SP).");
    }
}
