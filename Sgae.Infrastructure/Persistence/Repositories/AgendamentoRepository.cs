using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AppDbContext _context;

    public AgendamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Agendamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos
            .Include(a => a.Lead)
            .Include(a => a.Atendimento)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Agendamento>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos
            .Include(a => a.Lead)
            .Include(a => a.Atendimento)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        await _context.Agendamentos.AddAsync(agendamento, cancellationToken);
    }

    public void Update(Agendamento agendamento)
    {
        _context.Agendamentos.Update(agendamento);
    }

    public void Delete(Agendamento agendamento)
    {
        agendamento.Delete(); // Soft Delete
        _context.Agendamentos.Update(agendamento);
    }
}
