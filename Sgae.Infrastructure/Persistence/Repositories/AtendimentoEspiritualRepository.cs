using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório concreto para persistência de AtendimentoEspiritual.
/// </summary>
public class AtendimentoEspiritualRepository : Repository<AtendimentoEspiritual>, IAtendimentoEspiritualRepository
{
    public AtendimentoEspiritualRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AtendimentoEspiritual?> GetByAgendamentoIdAsync(Guid agendamentoId, CancellationToken cancellationToken = default)
    {
        return await Context.AtendimentosEspirituais
            .Include(a => a.Agendamento)
            .FirstOrDefaultAsync(a => a.AgendamentoId == agendamentoId, cancellationToken);
    }

    public override async Task<AtendimentoEspiritual?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.AtendimentosEspirituais
            .Include(a => a.Agendamento)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
