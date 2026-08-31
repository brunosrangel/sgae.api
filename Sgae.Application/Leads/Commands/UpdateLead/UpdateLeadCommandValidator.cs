using System;
using System.Linq;
using FluentValidation;

namespace Sgae.Application.Leads.Commands.UpdateLead;

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID do consulente é obrigatório para atualização.");

        RuleFor(v => v.GetNomeEfetivo())
            .NotEmpty().WithMessage("Nome/NomeCompleto do consulente é obrigatório.")
            .MaximumLength(150).WithMessage("Nome não pode exceder 150 caracteres.");

        RuleFor(v => v.Telefone)
            .NotEmpty().WithMessage("Telefone para contato é obrigatório.")
            .MaximumLength(30).WithMessage("Telefone não pode exceder 30 caracteres.")
            .Matches(@"^\+?[0-9\(\)\s\-\.]{8,30}$").WithMessage("Telefone em formato inválido. Use caracteres numéricos, parênteses, espaço ou hífens (ex: 11987654321 ou (11) 99999-9999).");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .MaximumLength(120).WithMessage("Email não pode exceder 120 caracteres.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(v => v.DataNascimento)
            .Must(d => !d.HasValue || d.Value.Date <= DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("A data de nascimento não pode ser uma data futura.")
            .Must(d => !d.HasValue || d.Value.Date >= DateTime.UtcNow.Date.AddYears(-130))
            .WithMessage("A data de nascimento informada é inválida.");

        RuleFor(v => v.TradicaoTerreiro)
            .MaximumLength(250).WithMessage("Tradição de terreiro não pode exceder 250 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.TradicaoTerreiro));

        RuleFor(v => v.VinculoTradicoes)
            .MaximumLength(200).WithMessage("Vínculo de tradições não pode exceder 200 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.VinculoTradicoes));

        RuleFor(v => v.VinculoCcrias)
            .MaximumLength(150).WithMessage("Vínculo CCRIAS não pode exceder 150 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.VinculoCcrias));

        RuleFor(v => v.Temporalidade)
            .MaximumLength(100).WithMessage("Temporalidade não pode exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Temporalidade));

        RuleFor(v => v.JogouBuziosBabalorisaSidnei)
            .MaximumLength(50).WithMessage("Campo 'jogouBuziosBabalorisaSidnei' não pode exceder 50 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.JogouBuziosBabalorisaSidnei));

        RuleFor(v => v.OrixasNagoKetu)
            .Must(list => list == null || list.All(o => string.IsNullOrWhiteSpace(o) || o.Length <= 100))
            .WithMessage("Cada orixá informado não pode exceder 100 caracteres.");

        RuleFor(v => v.Profissao)
            .MaximumLength(150).WithMessage("Profissão não pode exceder 150 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Profissao));

        RuleFor(v => v.Nacionalidade)
            .MaximumLength(100).WithMessage("Nacionalidade não pode exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Nacionalidade));

        RuleFor(v => v.Naturalidade)
            .MaximumLength(100).WithMessage("Naturalidade não pode exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Naturalidade));

        RuleFor(v => v.Cep)
            .MaximumLength(20).WithMessage("CEP não pode exceder 20 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Cep));

        RuleFor(v => v.Endereco)
            .MaximumLength(250).WithMessage("Endereço não pode exceder 250 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Endereco));

        RuleFor(v => v.Numero)
            .MaximumLength(50).WithMessage("Número não pode exceder 50 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Numero));

        RuleFor(v => v.Complemento)
            .MaximumLength(100).WithMessage("Complemento não pode exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Complemento));

        RuleFor(v => v.Bairro)
            .MaximumLength(100).WithMessage("Bairro não pode exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Bairro));

        RuleFor(v => v.GetCidadeEfetiva())
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.");

        RuleFor(v => v.GetEstadoEfetivo())
            .NotEmpty().WithMessage("Estado (UF) é obrigatório.")
            .Length(2).WithMessage("Estado (UF) deve conter exatamente 2 caracteres (ex: SP).");
    }
}
