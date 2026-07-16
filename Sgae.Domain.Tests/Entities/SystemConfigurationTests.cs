using Sgae.Domain.Entities;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class SystemConfigurationTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateSystemConfiguration()
    {
        // Arrange
        var chave = "DatabaseCacheMinutes";
        var valor = "60";
        var descricao = "Tempo padrão de sobrevivência das listagens pastorais no cache distribuído.";

        // Act
        var config = new SystemConfiguration(chave, valor, descricao);

        // Assert
        Assert.NotNull(config);
        Assert.NotEqual(Guid.Empty, config.Id);
        Assert.Equal(chave, config.Chave);
        Assert.Equal(valor, config.Valor);
        Assert.Equal(descricao, config.Descricao);
        Assert.False(config.IsDeleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidChave_ShouldThrowArgumentException(string invalidChave)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SystemConfiguration(
            invalidChave,
            "120",
            "Descrição válida"
        ));
    }

    [Fact]
    public void UpdateValue_WithValidParameter_ShouldUpdateValueAndRegisterTime()
    {
        // Arrange
        var config = new SystemConfiguration("JwtExpirationMinutes", "120", "Expiração em minutos");
        var novoValor = "240";

        // Act
        config.UpdateValue(novoValor);

        // Assert
        Assert.Equal(novoValor, config.Valor);
        Assert.NotNull(config.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateValue_WithInvalidParameter_ShouldThrowArgumentException(string invalidValue)
    {
        // Arrange
        var config = new SystemConfiguration("JwtExpirationMinutes", "120", "Expiração em minutos");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => config.UpdateValue(invalidValue));
    }
}
