using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Interceptor do EF Core para Auditoria (Audit Log) abrangente e automática.
/// Registra todas as inserções, modificações e exclusões nas entidades de negócio (Atendimento, Anexo, etc.),
/// capturando a identidade do usuário (UserIdentity), IP, Correlation ID, carimbo de data/hora UTC, campos alterados
/// e seus respectivos valores antigos (OldValues) e novos (NewValues) em formato JSON.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICorrelationIdProvider? _correlationIdProvider;

    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "SenhaHash", "PasswordHash", "Token", "RefreshToken", "Base64Data"
    };

    public AuditSaveChangesInterceptor(
        ICurrentUserService currentUserService,
        ICorrelationIdProvider? correlationIdProvider = null)
    {
        _currentUserService = currentUserService;
        _correlationIdProvider = correlationIdProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AuditChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AuditChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditChanges(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (!entries.Any()) return;

        var userIdentity = _currentUserService.GetUserIdentity();
        var ipAddress = _currentUserService.IpAddress;
        var correlationId = _correlationIdProvider?.GetCorrelationId();
        var nowUtc = DateTime.UtcNow;

        var auditEntries = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            // Remover proxy names se aplicável
            if (entityName.Contains("Proxy"))
            {
                entityName = entry.Entity.GetType().BaseType?.Name ?? entityName;
            }

            var entityId = GetPrimaryKeyValue(entry);
            var action = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedProperties = new List<string>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Não logar propriedades sensíveis em claro ou payloads binários gigantes
                if (SensitiveProperties.Contains(propertyName))
                {
                    if (property.IsModified)
                    {
                        changedProperties.Add(propertyName);
                        oldValues[propertyName] = "[REDACTED]";
                        newValues[propertyName] = "[REDACTED]";
                    }
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            var original = property.OriginalValue;
                            var current = property.CurrentValue;

                            if (!Equals(original, current))
                            {
                                changedProperties.Add(propertyName);
                                oldValues[propertyName] = original;
                                newValues[propertyName] = current;
                            }
                        }
                        break;
                }
            }

            // Se for modified mas nenhuma propriedade mudou de fato, ignora
            if (entry.State == EntityState.Modified && !changedProperties.Any())
            {
                continue;
            }

            var changedColumnsJson = changedProperties.Any()
                ? JsonSerializer.Serialize(changedProperties)
                : null;

            var oldValuesJson = oldValues.Any()
                ? JsonSerializer.Serialize(oldValues)
                : null;

            var newValuesJson = newValues.Any()
                ? JsonSerializer.Serialize(newValues)
                : null;

            var auditLog = new AuditLog(
                entityName,
                entityId,
                action,
                userIdentity,
                nowUtc,
                changedColumnsJson,
                oldValuesJson,
                newValuesJson,
                ipAddress,
                correlationId
            );

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Any())
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        if (keyProperty?.CurrentValue != null)
        {
            return keyProperty.CurrentValue.ToString() ?? Guid.NewGuid().ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
