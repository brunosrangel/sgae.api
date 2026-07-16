using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

/// <summary>
/// Contrato de repositório específico para a entidade AtendimentoEspiritual.
/// </summary>
public interface IAtendimentoEspiritualRepository : IRepository<AtendimentoEspiritual>
{
    // Adicione métodos específicos se necessários para Atendimento Espiritual, ex: obter por agendamento
    Task<AtendimentoEspiritual?> GetByAgendamentoIdAsync(Guid agendamentoId, CancellationToken cancellationToken = default);
}
