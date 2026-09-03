using FluentAssertions;
using Sgae.Infrastructure.Services;
using Xunit;

namespace Sgae.Application.Tests.Usuarios;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ComSenhaValida_DeveGerarHashDiferenteDoTextoPlano()
    {
        // Arrange
        var senha = "MinhaSenhaSuperSegura2026!";

        // Act
        var hash = _hasher.HashPassword(senha);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(senha);
        hash.Split('.').Length.Should().Be(3);
    }

    [Fact]
    public void VerifyPassword_ComSenhaCorreta_DeveRetornarTrue()
    {
        // Arrange
        var senha = "SgaeAdmin2026!";
        var hash = _hasher.HashPassword(senha);

        // Act
        var valido = _hasher.VerifyPassword(senha, hash);

        // Assert
        valido.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ComSenhaIncorreta_DeveRetornarFalse()
    {
        // Arrange
        var senhaCorreta = "SgaeAdmin2026!";
        var senhaErrada = "SenhaIncorreta123!";
        var hash = _hasher.HashPassword(senhaCorreta);

        // Act
        var valido = _hasher.VerifyPassword(senhaErrada, hash);

        // Assert
        valido.Should().BeFalse();
    }
}
