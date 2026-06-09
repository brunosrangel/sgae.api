using System;
using System.Threading;
using System.Threading.Tasks;
using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

public interface IPerfilConsulenteRepository : IRepository<PerfilConsulente>
{
    Task<PerfilConsulente?> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);
}
