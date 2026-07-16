using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

/// <summary>
/// Contrato de repositório específico para a entidade Acompanhamento.
/// </summary>
public interface IAcompanhamentoRepository : IRepository<Acompanhamento>
{
    // Adicione métodos específicos se necessários para Acompanhamento, ex: obter por atendimento espiritual
    Task<IEnumerable<Acompanhamento>> GetByAtendimentoEspiritualIdAsync(Guid atendimentoEspiritualId, CancellationToken cancellationToken = default);
}
