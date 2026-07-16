using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class SpiritualAttendanceCategoryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateSpiritualAttendanceCategory()
    {
        // Arrange
        var tipo = TipoAtendimento.TratamentoEspiritual;
        var nome = "Tratamento Espiritual";
        var descricao = "Tratamento focado na harmonização energética.";
        var tempo = 40;

        // Act
        var category = new SpiritualAttendanceCategory(tipo, nome, descricao, tempo);

        // Assert
        Assert.NotNull(category);
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(tipo, category.Tipo);
        Assert.Equal(nome, category.Nome);
        Assert.Equal(descricao, category.Descricao);
        Assert.Equal(tempo, category.TempoRecomendadoMinutos);
        Assert.False(category.IsDeleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidNome_ShouldThrowArgumentException(string invalidNome)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SpiritualAttendanceCategory(
            TipoAtendimento.TratamentoEspiritual,
            invalidNome,
            "Descrição válida",
            40
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidDescricao_ShouldThrowArgumentException(string invalidDescricao)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SpiritualAttendanceCategory(
            TipoAtendimento.TratamentoEspiritual,
            "Nome Válido",
            invalidDescricao,
            40
        ));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_WithInvalidTempo_ShouldThrowArgumentException(int invalidTempo)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SpiritualAttendanceCategory(
            TipoAtendimento.TratamentoEspiritual,
            "Nome Válido",
            "Descrição Válida",
            invalidTempo
        ));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCategory()
    {
        // Arrange
        var category = new SpiritualAttendanceCategory(TipoAtendimento.Desobsessao, "Desobsessão", "Antiga descrição", 45);
        var novoNome = "Desobsessão Espiritual";
        var novaDescricao = "Nova descrição atualizada";
        var novoTempo = 60;

        // Act
        category.Update(novoNome, novaDescricao, novoTempo);

        // Assert
        Assert.Equal(novoNome, category.Nome);
        Assert.Equal(novaDescricao, category.Descricao);
        Assert.Equal(novoTempo, category.TempoRecomendadoMinutos);
        Assert.NotNull(category.UpdatedAt);
    }
}
