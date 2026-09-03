using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sgae.API.Middlewares;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Infrastructure.Persistence;
using Xunit;

namespace Sgae.Application.Tests.Usuarios;

public class UserActiveValidationMiddlewareTests
{
    private readonly Mock<ILogger<UserActiveValidationMiddleware>> _loggerMock = new();

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"SgaeTestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task InvokeAsync_UsuarioNaoAutenticado_DeveProsseguirPipeline()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserActiveValidationMiddleware(next, _loggerMock.Object);
        using var db = CreateInMemoryDbContext();

        // Act
        await middleware.InvokeAsync(context, db);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UsuarioAutenticadoEAtivo_DeveProsseguirPipeline()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var usuario = new Usuario("Pastor Lucas", "lucas@sgae.com", "hash123", PerfilUsuario.Sacerdote);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, "Sacerdote")
        }, "TestAuthType");

        context.User = new ClaimsPrincipal(identity);

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserActiveValidationMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, db);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UsuarioAutenticadoMasInativo_DeveRejeitarCom403Forbidden()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var usuario = new Usuario("Usuario Inativo", "inativo@sgae.com", "hash123", PerfilUsuario.Consulente);
        usuario.SetStatus(false);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, "Consulente")
        }, "TestAuthType");

        context.User = new ClaimsPrincipal(identity);

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserActiveValidationMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, db);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().Contain("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_UsuarioAutenticadoInexistenteNoBanco_DeveRejeitarCom401Unauthorized()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var nonExistentUserId = Guid.NewGuid();

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, nonExistentUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuthType");

        context.User = new ClaimsPrincipal(identity);

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserActiveValidationMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, db);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Contain("application/problem+json");
    }
}
