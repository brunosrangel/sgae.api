using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

public class PerfilConsulenteRepository : Repository<PerfilConsulente>, IPerfilConsulenteRepository
{
    public PerfilConsulenteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PerfilConsulente?> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await Context.PerfisConsulentes
            .FirstOrDefaultAsync(p => p.LeadId == leadId, cancellationToken);
    }
}
