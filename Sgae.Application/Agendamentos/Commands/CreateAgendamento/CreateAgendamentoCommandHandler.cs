using System;
using System.Threading;
using System.Threading.Tasks;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de criação de um agendamento espiritual.
/// </summary>
public class CreateAgendamentoCommandHandler : ICommandHandler<CreateAgendamentoCommand, Guid>
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgendamentoCommandHandler(
        IAgendamentoRepository agendamentoRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _agendamentoRepository = agendamentoRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAgendamentoCommand request, CancellationToken cancellationToken)
    {
        // 1. Garante a integridade lógica: Consulente deve existir previamente na base
        var leadExists = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken);

        if (leadExists == null)
        {
            throw new ArgumentException($"O consulente de ID '{request.LeadId}' não foi localizado no sistema.");
        }

        // 2. Resolve a data e hora em formato UTC seguro utilizando o helper de domínio
        DateTime dataHoraUtc = !string.IsNullOrWhiteSpace(request.Data)
            ? Sgae.Domain.Common.DateTimeHelper.ParseUtc(request.Data, request.Horario)
            : Sgae.Domain.Common.DateTimeHelper.EnsureUtc(request.DataHora);

        // 3. Instancia a entidade rica executando as regras de estado
        var agendamento = new Agendamento(
            request.LeadId,
            dataHoraUtc,
            request.Modalidade,
            request.Valor
        );

        await _agendamentoRepository.AddAsync(agendamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}