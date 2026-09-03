namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// DTO de visualização de registros de auditoria (Audit Log) para rastreabilidade de atendimentos.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserIdentity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ChangedColumns { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public Guid? CorrelationId { get; set; }
}
