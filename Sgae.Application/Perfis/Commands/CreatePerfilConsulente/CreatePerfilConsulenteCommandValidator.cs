using FluentValidation;

namespace Sgae.Application.Perfis.Commands.CreatePerfilConsulente;

/// <summary>
/// Validações estritas de entrada para o comando de criação do Perfil do Consulente.
/// </summary>
public class CreatePerfilConsulenteCommandValidator : AbstractValidator<CreatePerfilConsulenteCommand>
{
    public CreatePerfilConsulenteCommandValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("O identificador do Consulente (LeadId) é obrigatório.");

        RuleFor(x => x.Idade)
            .InclusiveBetween(1, 120).WithMessage("A idade informada deve ser válida, entre 1 e 120 anos.");

        RuleFor(x => x.FaixaEtaria)
            .NotEmpty().WithMessage("A faixa etária é obrigatória.");

        RuleFor(x => x.Genero)
            .NotEmpty().WithMessage("O gênero é obrigatório.");

        RuleFor(x => x.Profissao)
            .NotEmpty().WithMessage("A profissão é obrigatória.");

        RuleFor(x => x.Escolaridade)
            .NotEmpty().WithMessage("O nível de escolaridade é obrigatório.");

        RuleFor(x => x.EstadoCivil)
            .NotEmpty().WithMessage("O estado civil é obrigatório.");
    }
}
