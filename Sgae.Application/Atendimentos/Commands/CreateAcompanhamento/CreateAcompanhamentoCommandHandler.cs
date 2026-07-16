using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de criação de um Acompanhamento.
/// </summary>
public class CreateAcompanhamentoCommandHandler : ICommandHandler<CreateAcompanhamentoCommand, Guid>
{
    private readonly IAcompanhamentoRepository _acompanhamentoRepository;
    private readonly IAtendimentoEspiritualRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAcompanhamentoCommandHandler> _logger;
    private readonly IDistributedCache _cache;

    public CreateAcompanhamentoCommandHandler(
        IAcompanhamentoRepository acompanhamentoRepository,
        IAtendimentoEspiritualRepository atendimentoRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAcompanhamentoCommandHandler> logger,
        IDistributedCache cache)
    {
        _acompanhamentoRepository = acompanhamentoRepository;
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateAcompanhamentoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Serilog Auditoria: Iniciando registro de novo Acompanhamento para o Atendimento Espiritual ID {AtendimentoEspiritualId} na Data {DataAcompanhamento}.",
            request.AtendimentoEspiritualId,
            request.DataAcompanhamento);

        // 1. Validar que o AtendimentoEspiritual existe
        var atendimento = await _atendimentoRepository.GetByIdAsync(request.AtendimentoEspiritualId, cancellationToken);
        if (atendimento == null)
        {
            _logger.LogWarning(
                "Serilog Auditoria: Falha no registro do Acompanhamento. Atendimento Espiritual ID {AtendimentoEspiritualId} não foi localizado.",
                request.AtendimentoEspiritualId);
            throw new ArgumentException($"O Atendimento Espiritual de ID '{request.AtendimentoEspiritualId}' não foi localizado.");
        }

        // 2. Instanciar entidade de domínio com lógica de validação rica
        var acompanhamento = new Acompanhamento(
            request.AtendimentoEspiritualId,
            request.DataAcompanhamento,
            request.SintomasMelhora,
            request.Recomendacoes,
            request.Observacoes
        );

        await _acompanhamentoRepository.AddAsync(acompanhamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Serilog Auditoria: Acompanhamento de evolução registrado com absoluto sucesso. ID Gerado: {AcompanhamentoId} para Atendimento Espiritual ID {AtendimentoEspiritualId}.",
            acompanhamento.Id,
            request.AtendimentoEspiritualId);

        // Invalida o cache dos acompanhamentos deste atendimento específico
        await _cache.RemoveAsync($"acompanhamentos_atendimento_{request.AtendimentoEspiritualId}", cancellationToken);

        return acompanhamento.Id;
    }
}
