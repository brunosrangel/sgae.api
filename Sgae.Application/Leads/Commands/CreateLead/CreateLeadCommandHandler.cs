using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Manipulador que recebe o comando, cria a entidade rica e persiste via repositório e Unit of Work.
/// </summary>
public class CreateLeadCommandHandler : ICommandHandler<CreateLeadCommand, Guid>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        // Instanciação rica da entidade de domínio (validará as exigências e invariantes internamente)
        var lead = new Lead(
            request.Nome,
            request.Telefone,
            request.Email,
            request.Cidade,
            request.Estado,
            request.Origem,
            request.ProblemaPrincipal
        );

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}