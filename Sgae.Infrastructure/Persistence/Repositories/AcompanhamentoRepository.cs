using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório concreto para persistência de Acompanhamento.
/// </summary>
public class AcompanhamentoRepository : Repository<Acompanhamento>, IAcompanhamentoRepository
{
    public AcompanhamentoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Acompanhamento>> GetByAtendimentoEspiritualIdAsync(Guid atendimentoEspiritualId, CancellationToken cancellationToken = default)
    {
        return await Context.Acompanhamentos
            .Include(a => a.AtendimentoEspiritual)
            .Where(a => a.AtendimentoEspiritualId == atendimentoEspiritualId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Acompanhamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Acompanhamentos
            .Include(a => a.AtendimentoEspiritual)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
