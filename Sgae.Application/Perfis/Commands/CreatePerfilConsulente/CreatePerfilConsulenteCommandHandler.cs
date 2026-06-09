using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Perfis.Commands.CreatePerfilConsulente;

/// <summary>
/// Manipulador que processa o estabelecimento ou atualização do perfil demográfico do consulente.
/// </summary>
public class CreatePerfilConsulenteCommandHandler : ICommandHandler<CreatePerfilConsulenteCommand, Guid>
{
    private readonly IPerfilConsulenteRepository _perfilRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePerfilConsulenteCommandHandler> _logger;
    private readonly IDistributedCache _cache;

    public CreatePerfilConsulenteCommandHandler(
        IPerfilConsulenteRepository perfilRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePerfilConsulenteCommandHandler> logger,
        IDistributedCache cache)
    {
        _perfilRepository = perfilRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreatePerfilConsulenteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Serilog Auditoria: Iniciando processamento do Perfil do Consulente para Lead ID {LeadId}.",
            request.LeadId);

        // 1. Validar que o Lead (Consulente) existe e está ativo
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null)
        {
            _logger.LogWarning(
                "Serilog Auditoria: Falha ao registrar perfil. Consulente (LeadId) {LeadId} não localizado.",
                request.LeadId);
            throw new ArgumentException($"O consulente com ID '{request.LeadId}' não foi localizado no sistema.");
        }

        // 2. Verificar se o perfil já foi registrado anteriormente (Relacionamento 1-para-1)
        var existingPerfil = await _perfilRepository.GetByLeadIdAsync(request.LeadId, cancellationToken);
        Guid perfilId;

        if (existingPerfil != null)
        {
            _logger.LogInformation(
                "Serilog Auditoria: Perfil existente localizado para o Lead ID {LeadId}. Atualizando dados demográficos.",
                request.LeadId);

            // Atualização de estado da entidade rica aplicando regras de domínio estritas
            existingPerfil.UpdatePerfil(
                request.Idade,
                request.FaixaEtaria,
                request.Genero,
                request.Profissao,
                request.Escolaridade,
                request.EstadoCivil
            );

            _perfilRepository.Update(existingPerfil);
            perfilId = existingPerfil.Id;
        }
        else
        {
            _logger.LogInformation(
                "Serilog Auditoria: Estabelecendo novo perfil demográfico para o Lead ID {LeadId}.",
                request.LeadId);

            // Instancia nova entidade epidemiológica rica
            var novoPerfil = new PerfilConsulente(
                request.LeadId,
                request.Idade,
                request.FaixaEtaria,
                request.Genero,
                request.Profissao,
                request.Escolaridade,
                request.EstadoCivil
            );

            await _perfilRepository.AddAsync(novoPerfil, cancellationToken);
            lead.DefinirPerfil(novoPerfil);
            _leadRepository.Update(lead);
            perfilId = novoPerfil.Id;
        }

        // 3. Commit transacional coordenado por UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Invalidação de cache baseada em chaves
        await _cache.RemoveAsync($"lead_{request.LeadId}", cancellationToken);
        await _cache.RemoveAsync($"perfil_lead_{request.LeadId}", cancellationToken);

        _logger.LogInformation(
            "Serilog Auditoria: Perfil do Consulente salvo com sucesso para o Lead ID {LeadId}. Perfil ID: {PerfilId}",
            request.LeadId,
            perfilId);

        return perfilId;
    }
}
