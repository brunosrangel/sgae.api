using System;

namespace Sgae.Domain.Common;

/// <summary>
/// Classe abstrata base para todas as entidades de domínio com suporte à exclusão lógica e auditoria básica.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public void RegisterUpdate()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        RegisterUpdate();
    }
}