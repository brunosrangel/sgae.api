using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação concreta do repositório de Atendimentos e Anexos via Entity Framework Core.
/// </summary>
public class AtendimentoRepository : Repository<Atendimento>, IAtendimentoRepository
{
    public AtendimentoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Atendimento>> GetWithFiltersAsync(
        string? status = null,
        Guid? sacerdoteId = null,
        Guid? consulenteId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Atendimentos
            .Include(a => a.Sacerdote)
            .Include(a => a.Consulente)
            .Include(a => a.Agendamento)
            .Include(a => a.Anexos)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTrim = status.Trim();
            query = query.Where(a => a.Status.ToLower() == statusTrim.ToLower());
        }

        if (sacerdoteId.HasValue && sacerdoteId.Value != Guid.Empty)
        {
            query = query.Where(a => a.SacerdoteId == sacerdoteId.Value);
        }

        if (consulenteId.HasValue && consulenteId.Value != Guid.Empty)
        {
            query = query.Where(a => a.ConsulenteId == consulenteId.Value);
        }

        if (dataInicio.HasValue)
        {
            var inicio = DateTime.SpecifyKind(dataInicio.Value, DateTimeKind.Utc);
            query = query.Where(a => a.DataConsulta >= inicio);
        }

        if (dataFim.HasValue)
        {
            var fim = DateTime.SpecifyKind(dataFim.Value, DateTimeKind.Utc);
            query = query.Where(a => a.DataConsulta <= fim);
        }

        return await query
            .OrderByDescending(a => a.DataConsulta)
            .ToListAsync(cancellationToken);
    }

    public async Task<Atendimento?> GetByIdWithAnexosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Atendimentos
            .Include(a => a.Sacerdote)
            .Include(a => a.Consulente)
            .Include(a => a.Agendamento)
            .Include(a => a.Anexos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AnexoAtendimento?> GetAnexoByIdAsync(Guid atendimentoId, Guid anexoId, CancellationToken cancellationToken = default)
    {
        return await Context.AnexosAtendimento
            .FirstOrDefaultAsync(a => a.AtendimentoId == atendimentoId && a.Id == anexoId, cancellationToken);
    }

    public async Task<IEnumerable<AnexoAtendimento>> GetAnexosByAtendimentoIdAsync(Guid atendimentoId, CancellationToken cancellationToken = default)
    {
        return await Context.AnexosAtendimento
            .Where(a => a.AtendimentoId == atendimentoId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
