using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Leads.Commands.DeleteLead;

/// <summary>
/// Manipulador que processa a remoção do Consulente (Lead).
/// </summary>
public class DeleteLeadCommandHandler : ICommandHandler<DeleteLeadCommand>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
        {
            throw new ArgumentException($"Consulente de ID '{request.Id}' não localizado.");
        }

        _leadRepository.Delete(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
