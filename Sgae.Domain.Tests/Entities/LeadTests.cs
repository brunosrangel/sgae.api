using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class LeadTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLead()
    {
        // Arrange
        var nome = "Bruno Rangel";
        var telefone = "(21) 98888-7777";
        var email = "bruno@email.com";
        var cidade = "Rio de Janeiro";
        var estado = "RJ";
        var origem = OrigemContato.Instagram;
        var problemaPrincipal = "Busca de orientação sobre dilemas existenciais.";

        // Act
        var lead = new Lead(nome, telefone, email, cidade, estado, origem, problemaPrincipal);

        // Assert
        Assert.NotNull(lead);
        Assert.NotEqual(Guid.Empty, lead.Id);
        Assert.Equal(nome, lead.Nome);
        Assert.Equal(telefone, lead.Telefone);
        Assert.Equal(email, lead.Email);
        Assert.Equal(cidade, lead.Cidade);
        Assert.Equal(estado, lead.Estado);
        Assert.Equal(origem, lead.Origem);
        Assert.Equal(problemaPrincipal, lead.ProblemaPrincipal);
        Assert.False(lead.IsDeleted);
        Assert.True(lead.CreatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullNome_ShouldThrowArgumentException(string invalidNome)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            invalidNome,
            "(21) 98888-7777",
            "bruno@email.com",
            "Rio de Janeiro",
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullTelefone_ShouldThrowArgumentException(string invalidTelefone)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            invalidTelefone,
            "bruno@email.com",
            "Rio de Janeiro",
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullCidade_ShouldThrowArgumentException(string invalidCidade)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            "(21) 98888-7777",
            "bruno@email.com",
            invalidCidade,
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("R")]
    [InlineData("RJO")]
    [InlineData(null)]
    public void Constructor_WithInvalidEstadoLength_ShouldThrowArgumentException(string invalidEstado)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            "(21) 98888-7777",
            "bruno@email.com",
            "Rio de Janeiro",
            invalidEstado,
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Fact]
    public void Delete_ShouldMarkAsDeletedAndRegisterUpdate()
    {
        // Arrange
        var lead = new Lead("Bruno", "(21) 98888-7777", "bruno@email.com", "Rio", "RJ", OrigemContato.Instagram, "Problema");

        // Act
        lead.Delete();

        // Assert
        Assert.True(lead.IsDeleted);
        Assert.NotNull(lead.UpdatedAt);
    }
}
