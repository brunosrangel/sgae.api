using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Atendimentos.Commands.CreateAtendimento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de registro de um Atendimento Espiritual.
/// </summary>
public class CreateAtendimentoCommandHandler : ICommandHandler<CreateAtendimentoCommand, Guid>
{
    private readonly IAtendimentoEspiritualRepository _atendimentoRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAtendimentoCommandHandler> _logger;
    private readonly IDistributedCache _cache;

    public CreateAtendimentoCommandHandler(
        IAtendimentoEspiritualRepository atendimentoRepository,
        IAgendamentoRepository agendamentoRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAtendimentoCommandHandler> logger,
        IDistributedCache cache)
    {
        _atendimentoRepository = atendimentoRepository;
        _agendamentoRepository = agendamentoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateAtendimentoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Serilog Auditoria: Iniciando registro de Atendimento Espiritual para o Agendamento ID {AgendamentoId} com Tipo {TipoAtendimento}.",
            request.AgendamentoId,
            request.Tipo);

        // 1. Validar que o Agendamento existe e está num estado elegível
        var agendamento = await _agendamentoRepository.GetByIdAsync(request.AgendamentoId, cancellationToken);
        if (agendamento == null)
        {
            _logger.LogWarning(
                "Serilog Auditoria: Falha no registro de Atendimento. Agendamento ID {AgendamentoId} não foi localizado.",
                request.AgendamentoId);
            throw new ArgumentException($"O agendamento de ID '{request.AgendamentoId}' não foi localizado no sistema.");
        }

        // Se o agendamento já tiver um atendimento, lançar erro
        var existingAtendimento = await _atendimentoRepository.GetByAgendamentoIdAsync(request.AgendamentoId, cancellationToken);
        if (existingAtendimento != null)
        {
            _logger.LogWarning(
                "Serilog Auditoria: Transação Recusada. Já existe um Atendimento Espiritual ID {AtendimentoId} registrado para o Agendamento ID {AgendamentoId}.",
                existingAtendimento.Id,
                request.AgendamentoId);
            throw new InvalidOperationException("Já existe um Atendimento Espiritual registrado para este agendamento.");
        }

        // Marcar o agendamento como realizado, se estiver Confirmado, para manter a consistência de status
        var oldStatus = agendamento.Status;
        if (agendamento.Status == Domain.Enums.StatusAgendamento.Confirmado)
        {
            agendamento.RealizarAgendamento();
            _agendamentoRepository.Update(agendamento);
            _logger.LogInformation(
                "Serilog Auditoria: Transicionando status do Agendamento ID {AgendamentoId} de {OldStatus} para {NewStatus}.",
                agendamento.Id,
                oldStatus,
                agendamento.Status);
        }
        else if (agendamento.Status == Domain.Enums.StatusAgendamento.Pendente)
        {
            agendamento.ConfirmarAgendamento();
            agendamento.RealizarAgendamento();
            _agendamentoRepository.Update(agendamento);
            _logger.LogInformation(
                "Serilog Auditoria: Transicionando status de Agendamento Pendente ID {AgendamentoId} para Confirmado e depois Realizado.",
                agendamento.Id);
        }

        // 2. Criar a entidade rica resolvendo regras de negócio
        var atendimento = new AtendimentoEspiritual(
            request.AgendamentoId,
            request.Tipo,
            request.TempoDuracaoMinutos,
            request.TemasAbordados,
            request.Observacoes
        );

        await _atendimentoRepository.AddAsync(atendimento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Serilog Auditoria: Atendimento Espiritual registrado com absoluto sucesso. ID Gerado: {AtendimentoId} associado ao Agendamento ID {AgendamentoId}.",
            atendimento.Id,
            request.AgendamentoId);

        // Invalida cache de lista de atendimentos pois a lista mudou
        await _cache.RemoveAsync("atendimentos_all", cancellationToken);

        return atendimento.Id;
    }
}
