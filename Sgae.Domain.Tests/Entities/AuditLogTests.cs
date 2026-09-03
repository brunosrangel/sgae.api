using FluentAssertions;
using Sgae.Domain.Entities;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAuditLog()
    {
        // Arrange
        var entityName = "Atendimento";
        var entityId = Guid.NewGuid().ToString();
        var action = "UPDATE";
        var userIdentity = "sacerdote@sgae.org";
        var timestamp = DateTime.UtcNow;
        var changedColumns = "[\"VeredictoEspiritual\",\"Status\"]";
        var oldValues = "{\"Status\":\"Pendente\"}";
        var newValues = "{\"Status\":\"Realizado\"}";
        var ipAddress = "192.168.1.100";
        var correlationId = Guid.NewGuid();

        // Act
        var auditLog = new AuditLog(
            entityName,
            entityId,
            action,
            userIdentity,
            timestamp,
            changedColumns,
            oldValues,
            newValues,
            ipAddress,
            correlationId
        );

        // Assert
        auditLog.Should().NotBeNull();
        auditLog.Id.Should().NotBe(Guid.Empty);
        auditLog.EntityName.Should().Be(entityName);
        auditLog.EntityId.Should().Be(entityId);
        auditLog.Action.Should().Be(action);
        auditLog.UserIdentity.Should().Be(userIdentity);
        auditLog.Timestamp.Should().Be(timestamp);
        auditLog.ChangedColumns.Should().Be(changedColumns);
        auditLog.OldValues.Should().Be(oldValues);
        auditLog.NewValues.Should().Be(newValues);
        auditLog.IpAddress.Should().Be(ipAddress);
        auditLog.CorrelationId.Should().Be(correlationId);
        auditLog.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithBlankEntityNameOrUser_ShouldFallbackToDefaults()
    {
        // Arrange & Act
        var auditLog = new AuditLog(
            "",
            "",
            "",
            "",
            DateTime.UtcNow
        );

        // Assert
        auditLog.EntityName.Should().Be("Desconhecido");
        auditLog.EntityId.Should().BeEmpty();
        auditLog.Action.Should().Be("UPDATE");
        auditLog.UserIdentity.Should().Be("Sistema");
    }
}
