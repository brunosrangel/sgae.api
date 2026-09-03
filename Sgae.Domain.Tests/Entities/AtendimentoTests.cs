using FluentAssertions;
using Sgae.Domain.Entities;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class AtendimentoTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAtendimento()
    {
        // Arrange
        var sacerdoteId = Guid.NewGuid();
        var consulenteId = Guid.NewGuid();
        var dataConsulta = DateTime.UtcNow;
        var tipoOraculo = "Jogo de Búzios";
        var perguntaCentral = "Orientação sobre caminhos profissionais e espirituais.";
        var veredictoEspiritual = "Causa raiz identificada no odu Obará; necessidade de ebó de abertura.";
        var agendamentoId = Guid.NewGuid();
        var status = "Realizado";
        var observacoes = "Consulente atento e receptivo.";

        // Act
        var atendimento = new Atendimento(
            sacerdoteId,
            consulenteId,
            dataConsulta,
            tipoOraculo,
            perguntaCentral,
            veredictoEspiritual,
            agendamentoId,
            status,
            observacoes
        );

        // Assert
        atendimento.Should().NotBeNull();
        atendimento.Id.Should().NotBe(Guid.Empty);
        atendimento.SacerdoteId.Should().Be(sacerdoteId);
        atendimento.ConsulenteId.Should().Be(consulenteId);
        atendimento.DataConsulta.Should().Be(dataConsulta);
        atendimento.TipoOraculo.Should().Be(tipoOraculo);
        atendimento.PerguntaCentral.Should().Be(perguntaCentral);
        atendimento.VeredictoEspiritual.Should().Be(veredictoEspiritual);
        atendimento.AgendamentoId.Should().Be(agendamentoId);
        atendimento.Status.Should().Be("Realizado");
        atendimento.Observacoes.Should().Be(observacoes);
        atendimento.Anexos.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptySacerdoteId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => new Atendimento(
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Jogo de Búzios",
            "Pergunta válida",
            "Veredicto válido com mais de 10 caracteres"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O Sacerdote responsável deve ser informado.");
    }

    [Fact]
    public void Constructor_WithEmptyConsulenteId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => new Atendimento(
            Guid.NewGuid(),
            Guid.Empty,
            DateTime.UtcNow,
            "Jogo de Búzios",
            "Pergunta válida",
            "Veredicto válido com mais de 10 caracteres"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O Consulente deve ser informado.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")] // menos de 5 caracteres
    public void Constructor_WithShortOrEmptyPerguntaCentral_ShouldThrowArgumentException(string invalidPergunta)
    {
        // Arrange & Act
        Action act = () => new Atendimento(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Jogo de Búzios",
            invalidPergunta,
            "Veredicto válido com mais de 10 caracteres"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Pergunta Central*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("curto")] // menos de 10 caracteres
    public void Constructor_WithShortOrEmptyVeredictoEspiritual_ShouldThrowArgumentException(string invalidVeredicto)
    {
        // Arrange & Act
        Action act = () => new Atendimento(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Jogo de Búzios",
            "Pergunta válida",
            invalidVeredicto
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Veredicto Espiritual*");
    }

    [Fact]
    public void AddAnexo_ShouldAddAnexoToCollection()
    {
        // Arrange
        var atendimento = new Atendimento(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Jogo de Búzios",
            "Pergunta válida",
            "Veredicto válido com mais de 10 caracteres"
        );

        var anexo = new AnexoAtendimento(
            atendimento.Id,
            "caida_buzios_01.jpg",
            "image/jpeg",
            102400,
            "base64data",
            "Primeira jogada - Odu Obará",
            90,
            "FotoBuzios"
        );

        // Act
        atendimento.AddAnexo(anexo);

        // Assert
        atendimento.Anexos.Should().HaveCount(1);
        atendimento.Anexos.First().NomeArquivo.Should().Be("caida_buzios_01.jpg");
        atendimento.Anexos.First().RotacaoGraus.Should().Be(90);
    }

    [Fact]
    public void RemoveAnexo_ShouldRemoveExistingAnexo()
    {
        // Arrange
        var atendimento = new Atendimento(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Jogo de Búzios",
            "Pergunta válida",
            "Veredicto válido com mais de 10 caracteres"
        );

        var anexo = new AnexoAtendimento(
            atendimento.Id,
            "caida_buzios_01.jpg",
            "image/jpeg",
            102400
        );
        atendimento.AddAnexo(anexo);

        // Act
        atendimento.RemoveAnexo(anexo.Id);

        // Assert
        atendimento.Anexos.Should().BeEmpty();
    }
}
