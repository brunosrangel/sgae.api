using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;

namespace Sgae.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Lead> Leads { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<PerfilConsulente> PerfisConsulentes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}