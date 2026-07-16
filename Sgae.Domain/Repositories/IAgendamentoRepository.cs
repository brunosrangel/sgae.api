using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

public interface IAgendamentoRepository
{
    Task<Agendamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Agendamento>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
    void Update(Agendamento agendamento);
    void Delete(Agendamento agendamento);
}
