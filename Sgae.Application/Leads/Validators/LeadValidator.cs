using FluentValidation;
using Sgae.Domain.Entities;

namespace Sgae.Application.Leads.Validators;

public class LeadValidator : AbstractValidator<Lead>
{
    public LeadValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do consulente é obrigatório.")
            .MaximumLength(150).WithMessage("Nome do consulente não pode exceder 150 caracteres.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone para contato é obrigatório.")
            .MaximumLength(25).WithMessage("Telefone não pode exceder 25 caracteres.")
            .Matches(@"^\+?[0-9\(\)\s\-]{8,25}$").WithMessage("Telefone em formato inválido. Use caracteres numéricos, parênteses, espaço ou hífens.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email do consulente é obrigatório.")
            .MaximumLength(100).WithMessage("Email não pode exceder 100 caracteres.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve conter exatamente 2 caracteres (UF).");

        RuleFor(x => x.ProblemaPrincipal)
            .NotEmpty().WithMessage("O problema principal deve ser especificado para captação.")
            .MaximumLength(1000).WithMessage("O problema principal não pode exceder 1000 caracteres.");
    }
}
