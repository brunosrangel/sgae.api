using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

/// <summary>
/// Contrato de repositório específico para a entidade Atendimento e seus anexos multimídia/litúrgicos.
/// </summary>
public interface IAtendimentoRepository : IRepository<Atendimento>
{
    Task<IEnumerable<Atendimento>> GetWithFiltersAsync(
        string? status = null,
        Guid? sacerdoteId = null,
        Guid? consulenteId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default);

    Task<Atendimento?> GetByIdWithAnexosAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AnexoAtendimento?> GetAnexoByIdAsync(Guid atendimentoId, Guid anexoId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AnexoAtendimento>> GetAnexosByAtendimentoIdAsync(Guid atendimentoId, CancellationToken cancellationToken = default);
}
