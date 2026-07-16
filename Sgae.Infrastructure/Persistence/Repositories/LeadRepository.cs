using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly AppDbContext _context;

    public LeadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Perfil)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Perfil)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
    }

    public void Update(Lead lead)
    {
        _context.Leads.Update(lead);
    }

    public void Delete(Lead lead)
    {
        lead.Delete(); // Soft Delete
        _context.Leads.Update(lead);
    }
}
