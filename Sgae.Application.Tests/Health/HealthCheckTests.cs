using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Sgae.API.Middlewares;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Infrastructure.Persistence;
using Xunit;

namespace Sgae.Application.Tests.Health;

public class HealthCheckTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"SgaeHealthTestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task IdentityHealthCheck_ComAdminAtivoEConfigValida_DeveRetornarHealthy()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var admin = new Usuario("Admin Sgae", "admin@sgae.com", "hash123", PerfilUsuario.Admin);
        dbContext.Usuarios.Add(admin);
        await dbContext.SaveChangesAsync();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "SuperSecretKeyForSgaeSystem2026ValidationAndSecuritySignatures!" },
            { "Jwt:Issuer", "SGAE.API" },
            { "Jwt:Audience", "SGAE.API" },
            { "Jwt:ExpirationMinutes", "120" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var tokenServiceMock = new Mock<ITokenService>();

        var healthCheck = new IdentityHealthCheck(dbContext, configuration, tokenServiceMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("IdentityStoreReady");
        result.Data["IdentityStoreReady"].Should().Be(true);
        result.Data.Should().ContainKey("ActiveAdminsCount");
        result.Data["ActiveAdminsCount"].Should().Be(1);
    }

    [Fact]
    public async Task IdentityHealthCheck_SemAdminCadastrado_DeveRetornarDegraded()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var consulente = new Usuario("Consulente", "consulente@sgae.com", "hash123", PerfilUsuario.Consulente);
        dbContext.Usuarios.Add(consulente);
        await dbContext.SaveChangesAsync();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "SuperSecretKeyForSgaeSystem2026ValidationAndSecuritySignatures!" },
            { "Jwt:Issuer", "SGAE.API" },
            { "Jwt:Audience", "SGAE.API" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var tokenServiceMock = new Mock<ITokenService>();

        var healthCheck = new IdentityHealthCheck(dbContext, configuration, tokenServiceMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("não foi detectado nenhum administrador ativo");
    }

    [Fact]
    public async Task DatabaseHealthCheck_ComBancoOperacional_DeveRetornarHealthy()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var healthCheck = new DatabaseHealthCheck(dbContext);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("Provider");
        result.Data.Should().ContainKey("ResponseTimeMs");
    }

    [Fact]
    public async Task SystemHealthCheck_ExecutandoNormalmente_DeveRetornarHealthyComMetricas()
    {
        // Arrange
        var healthCheck = new SystemHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("AllocatedMemoryMB");
        result.Data.Should().ContainKey("UptimeFormatted");
        result.Data.Should().ContainKey("DotNetVersion");
    }
}
