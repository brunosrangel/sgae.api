using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Leads.Commands.UpdateLead;

/// <summary>
/// Manipulador que processa a atualização do Consulente (Lead).
/// </summary>
public class UpdateLeadCommandHandler : ICommandHandler<UpdateLeadCommand>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
        {
            throw new ArgumentException($"Consulente de ID '{request.Id}' não localizado.");
        }

        lead.UpdateDadosPessoais(request.Nome, request.Email, request.Telefone);
        lead.UpdateLocalizacao(request.Cidade, request.Estado);
        lead.AlterarProblemaPrincipal(request.ProblemaPrincipal);

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
