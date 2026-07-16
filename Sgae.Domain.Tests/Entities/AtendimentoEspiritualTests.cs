using FluentAssertions;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class AtendimentoEspiritualTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAtendimentoEspiritual()
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var tipo = TipoAtendimento.TratamentoEspiritual;
        var tempoDuracaoMinutos = 45;
        var temasAbordados = "Obsessão espiritual e passes magnéticos.";
        var observacoes = "Consulente apresentou melhoras significativas no final da sessão.";

        // Act
        var atendimento = new AtendimentoEspiritual(agendamentoId, tipo, tempoDuracaoMinutos, temasAbordados, observacoes);

        // Assert
        atendimento.Should().NotBeNull();
        atendimento.Id.Should().NotBe(Guid.Empty);
        atendimento.AgendamentoId.Should().Be(agendamentoId);
        atendimento.Tipo.Should().Be(tipo);
        atendimento.TempoDuracaoMinutos.Should().Be(tempoDuracaoMinutos);
        atendimento.TemasAbordados.Should().Be(temasAbordados);
        atendimento.Observacoes.Should().Be(observacoes);
        atendimento.IsDeleted.Should().BeFalse();
        atendimento.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyAgendamentoId_ShouldThrowArgumentException()
    {
        // Arrange
        var agendamentoId = Guid.Empty;
        var tipo = TipoAtendimento.Desobsessao;
        var tempoDuracaoMinutos = 30;
        var temasAbordados = "Doutrinação";
        var observacoes = "obs";

        // Act
        Action act = () => new AtendimentoEspiritual(agendamentoId, tipo, tempoDuracaoMinutos, temasAbordados, observacoes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O atendimento deve estar vinculado a um Agendamento válido.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-15)]
    public void Constructor_WithNonPositiveTempoDuracao_ShouldThrowArgumentException(int invalidTempo)
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var tipo = TipoAtendimento.AssistenciaFraterna;
        var temasAbordados = "Temas gerais";
        var observacoes = "obs";

        // Act
        Action act = () => new AtendimentoEspiritual(agendamentoId, tipo, invalidTempo, temasAbordados, observacoes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O tempo de duração do atendimento deve ser positivo.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullTemasAbordados_ShouldThrowArgumentException(string invalidTemas)
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var tipo = TipoAtendimento.PasseEspiritual;
        var tempoDuracaoMinutos = 10;
        var observacoes = "obs";

        // Act
        Action act = () => new AtendimentoEspiritual(agendamentoId, tipo, tempoDuracaoMinutos, invalidTemas!, observacoes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Os temas abordados devem ser obrigatoriamente especificados.");
    }

    [Fact]
    public void UpdateAtendimento_WithValidParameters_ShouldUpdateFieldsAndRegisterUpdate()
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var atendimento = new AtendimentoEspiritual(agendamentoId, TipoAtendimento.Doutrinacao, 50, "Tema inicial", "Obs inicial");
        var novoTipo = TipoAtendimento.Outros;
        var novoTempo = 60;
        var novosTemas = "Tema alterado";
        var novasObs = "Obs alterada";

        // Act
        atendimento.UpdateAtendimento(novoTipo, novoTempo, novosTemas, novasObs);

        // Assert
        atendimento.Tipo.Should().Be(novoTipo);
        atendimento.TempoDuracaoMinutos.Should().Be(novoTempo);
        atendimento.TemasAbordados.Should().Be(novosTemas);
        atendimento.Observacoes.Should().Be(novasObs);
        atendimento.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateAtendimento_WithNonPositiveTempo_ShouldThrowArgumentException(int invalidTempo)
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var atendimento = new AtendimentoEspiritual(agendamentoId, TipoAtendimento.Doutrinacao, 50, "Tema inicial", "Obs inicial");

        // Act
        Action act = () => atendimento.UpdateAtendimento(TipoAtendimento.Desobsessao, invalidTempo, "Tema", "Obs");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("O tempo de duração do atendimento deve ser positivo.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateAtendimento_WithEmptyOrNullTemas_ShouldThrowArgumentException(string invalidTemas)
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var atendimento = new AtendimentoEspiritual(agendamentoId, TipoAtendimento.Doutrinacao, 50, "Tema inicial", "Obs inicial");

        // Act
        Action act = () => atendimento.UpdateAtendimento(TipoAtendimento.Desobsessao, 20, invalidTemas!, "Obs");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Os temas abordados devem ser obrigatoriamente especificados.");
    }
}
