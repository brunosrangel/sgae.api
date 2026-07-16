using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Atendimentos.Commands.DeleteAtendimento;

/// <summary>
/// Manipulador que processa a remoção do Atendimento Espiritual.
/// </summary>
public class DeleteAtendimentoCommandHandler : ICommandHandler<DeleteAtendimentoCommand>
{
    private readonly IAtendimentoEspiritualRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAtendimentoCommandHandler(IAtendimentoEspiritualRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAtendimentoCommand request, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (atendimento == null)
        {
            throw new ArgumentException($"Atendimento Espiritual de ID '{request.Id}' não localizado.");
        }

        _atendimentoRepository.Delete(atendimento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
