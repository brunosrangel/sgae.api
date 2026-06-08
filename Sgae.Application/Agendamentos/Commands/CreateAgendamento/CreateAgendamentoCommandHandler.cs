using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de criação de um agendamento espiritual.
/// </summary>
public class CreateAgendamentoCommandHandler : ICommandHandler<CreateAgendamentoCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgendamentoCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAgendamentoCommand request, CancellationToken cancellationToken)
    {
        // 1. Garante a integridade lógica: Consulente deve existir previamente na base
        var leadExists = await _context.Leads
            .AnyAsync(l => l.Id == request.LeadId, cancellationToken);

        if (!leadExists)
        {
            throw new ArgumentException($"O consulente de ID '{request.LeadId}' não foi localizado no sistema.");
        }

        // 2. Instancia a entidade rica executando as regras de estado
        var agendamento = new Agendamento(
            request.LeadId,
            request.DataHora,
            request.Modalidade,
            request.Valor
        );

        await _context.Agendamentos.AddAsync(agendamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}