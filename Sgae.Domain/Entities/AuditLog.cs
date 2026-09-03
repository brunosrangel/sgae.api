using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade de Auditoria (Audit Log) para rastreabilidade de conformidade e segurança.
/// Registra todas as inserções, modificações e exclusões realizadas em registros de Atendimentos e entidades críticas,
/// armazenando a identidade do operador (UserIdentity), campos alterados, valores anteriores e novos, IP e carimbo de tempo UTC.
/// </summary>
public class AuditLog : BaseEntity
{
    private AuditLog() { }

    public AuditLog(
        string entityName,
        string entityId,
        string action,
        string userIdentity,
        DateTime timestamp,
        string? changedColumns = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        Guid? correlationId = null)
    {
        EntityName = !string.IsNullOrWhiteSpace(entityName) ? entityName.Trim() : "Desconhecido";
        EntityId = !string.IsNullOrWhiteSpace(entityId) ? entityId.Trim() : string.Empty;
        Action = !string.IsNullOrWhiteSpace(action) ? action.Trim().ToUpperInvariant() : "UPDATE";
        UserIdentity = !string.IsNullOrWhiteSpace(userIdentity) ? userIdentity.Trim() : "Sistema";
        Timestamp = timestamp.Kind == DateTimeKind.Utc ? timestamp : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        ChangedColumns = changedColumns;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        CorrelationId = correlationId;
    }

    public string EntityName { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string Action { get; private set; } = null!; // INSERT, UPDATE, DELETE
    public string UserIdentity { get; private set; } = null!; // Email ou ID do Usuário Autenticado
    public DateTime Timestamp { get; private set; }
    public string? ChangedColumns { get; private set; } // JSON array ou lista de campos alterados
    public string? OldValues { get; private set; } // JSON com estado anterior
    public string? NewValues { get; private set; } // JSON com novo estado
    public string? IpAddress { get; private set; }
    public Guid? CorrelationId { get; private set; }
}
