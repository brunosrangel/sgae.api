using System;
using System.Threading;
using System.Threading.Tasks;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Agendamentos.Commands.DeleteAgendamento;

public class DeleteAgendamentoCommandHandler : ICommandHandler<DeleteAgendamentoCommand, bool>
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAgendamentoCommandHandler(
        IAgendamentoRepository agendamentoRepository,
        IUnitOfWork unitOfWork)
    {
        _agendamentoRepository = agendamentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAgendamentoCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _agendamentoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (agendamento == null)
        {
            throw new ArgumentException($"Agendamento com ID '{request.Id}' não foi localizado.");
        }

        agendamento.Delete();
        _agendamentoRepository.Update(agendamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
