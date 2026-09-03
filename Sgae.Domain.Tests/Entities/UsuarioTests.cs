using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void CriarUsuario_ComDadosValidos_DeveInstanciarComSucesso()
    {
        // Arrange & Act
        var usuario = new Usuario(
            "Maria Padilha",
            "maria@sgae.com",
            "hash123",
            PerfilUsuario.Secretaria
        );

        // Assert
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("Maria Padilha", usuario.Nome);
        Assert.Equal("maria@sgae.com", usuario.Email);
        Assert.Equal("hash123", usuario.PasswordHash);
        Assert.Equal(PerfilUsuario.Secretaria, usuario.Perfil);
        Assert.True(usuario.StatusAtivo);
        Assert.False(usuario.PrimeiroAcesso);
        Assert.Null(usuario.UltimoAcesso);
    }

    [Theory]
    [InlineData("", "teste@sgae.com", "hash")]
    [InlineData("Nome", "", "hash")]
    [InlineData("Nome", "teste@sgae.com", "")]
    public void CriarUsuario_ComDadosInvalidos_DeveLancarArgumentException(string nome, string email, string hash)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Usuario(nome, email, hash, PerfilUsuario.Admin));
    }

    [Fact]
    public void UpdatePassword_DeveAtualizarHashERegistrarUpdate()
    {
        // Arrange
        var usuario = new Usuario("Admin", "admin@sgae.com", "hashAntigo", PerfilUsuario.Admin, primeiroAcesso: true);

        // Act
        usuario.UpdatePassword("novoHashSeguro");

        // Assert
        Assert.Equal("novoHashSeguro", usuario.PasswordHash);
        Assert.False(usuario.PrimeiroAcesso);
        Assert.NotNull(usuario.UpdatedAt);
    }

    [Fact]
    public void RefreshToken_DeveGerenciarEstadoAtivoERevogado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken("token-criptografico-123", usuarioId, expiresAt, "127.0.0.1");

        // Assert
        Assert.True(token.IsActive);
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired);

        // Act
        token.Revoke("127.0.0.1", "Logout realizado", "novo-token");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("Logout realizado", token.ReasonRevoked);
        Assert.Equal("novo-token", token.ReplacedByToken);
    }
}
