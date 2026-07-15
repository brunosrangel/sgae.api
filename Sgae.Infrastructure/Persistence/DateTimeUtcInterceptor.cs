using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sgae.Domain.Common;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Interceptor do EF Core para detectar propriedades DateTime em entidades que estão sendo adicionadas ou modificadas
/// e automaticamente convertê-las/garantir que estejam em DateTimeKind.Utc antes de salvar no PostgreSQL.
/// </summary>
public class DateTimeUtcInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ConvertDateTimesToUtc(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ConvertDateTimesToUtc(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDateTimesToUtc(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var properties = entry.Metadata.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

            foreach (var property in properties)
            {
                var currentValue = entry.Property(property.Name).CurrentValue;

                if (currentValue is DateTime dt)
                {
                    entry.Property(property.Name).CurrentValue = DateTimeHelper.EnsureUtc(dt);
                }
            }
        }
    }
}
