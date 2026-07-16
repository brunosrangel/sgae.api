using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Atendimentos.Commands.UpdateAtendimento;

/// <summary>
/// Manipulador que processa a atualização do Atendimento Espiritual.
/// </summary>
public class UpdateAtendimentoCommandHandler : ICommandHandler<UpdateAtendimentoCommand>
{
    private readonly IAtendimentoEspiritualRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAtendimentoCommandHandler(IAtendimentoEspiritualRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateAtendimentoCommand request, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (atendimento == null)
        {
            throw new ArgumentException($"Atendimento Espiritual de ID '{request.Id}' não localizado.");
        }

        atendimento.UpdateAtendimento(
            request.Tipo,
            request.TempoDuracaoMinutos,
            request.TemasAbordados,
            request.Observacoes
        );

        _atendimentoRepository.Update(atendimento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
