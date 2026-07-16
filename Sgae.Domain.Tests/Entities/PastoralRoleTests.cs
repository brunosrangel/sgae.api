using Sgae.Domain.Entities;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class PastoralRoleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePastoralRole()
    {
        // Arrange
        var nome = "Coordenador";
        var descricao = "Coordenador do fluxo de consulentes e agendamentos.";
        var escopoPermissao = "Pastoral.Agendamento,Pastoral.Lead";

        // Act
        var role = new PastoralRole(nome, descricao, escopoPermissao);

        // Assert
        Assert.NotNull(role);
        Assert.NotEqual(Guid.Empty, role.Id);
        Assert.Equal(nome, role.Nome);
        Assert.Equal(descricao, role.Descricao);
        Assert.Equal(escopoPermissao, role.EscopoPermissao);
        Assert.False(role.IsDeleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidNome_ShouldThrowArgumentException(string invalidNome)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PastoralRole(
            invalidNome,
            "Descrição válida",
            "Permissão.Válida"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidDescricao_ShouldThrowArgumentException(string invalidDescricao)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PastoralRole(
            "Nome Válido",
            invalidDescricao,
            "Permissão.Válida"
        ));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdatePastoralRole()
    {
        // Arrange
        var role = new PastoralRole("Pastor", "Aconselhamento espiritual", "Pastoral.Atendimento");
        var novaDescricao = "Aconselhamento e acompanhamento espiritual intensivo";
        var novoEscopo = "Pastoral.Atendimento,Pastoral.Acompanhamento";

        // Act
        role.Update(novaDescricao, novoEscopo);

        // Assert
        Assert.Equal(novaDescricao, role.Descricao);
        Assert.Equal(novoEscopo, role.EscopoPermissao);
        Assert.NotNull(role.UpdatedAt);
    }
}
