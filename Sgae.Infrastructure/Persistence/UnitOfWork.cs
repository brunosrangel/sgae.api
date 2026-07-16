using Sgae.Application.Abstractions;

namespace Sgae.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(
        AppDbContext dbContext,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _context = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Persistir as alterações no Banco de Dados de forma transacional e consistente
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 2. Despachar os eventos de domínio registrados nas entidades ricas pós-autenticidade
        await _domainEventDispatcher.DispatchEventsAsync(cancellationToken);

        return result;
    }
}