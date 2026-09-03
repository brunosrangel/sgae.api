using FluentValidation.TestHelper;
using Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;
using Sgae.Application.Atendimentos.Commands.CreateAtendimento;
using Sgae.Application.Atendimentos.Commands.DeleteAtendimento;
using Sgae.Application.Atendimentos.Commands.UpdateAtendimento;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Atendimentos.Validators;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Application.Tests.Atendimentos.Validators;

public class SpiritualAttendanceValidatorsTests
{
    [Fact]
    public void CreateAtendimentoCommandValidator_Should_Pass_For_Valid_Data()
    {
        // Arrange
        var validator = new CreateAtendimentoCommandValidator();
        var command = new CreateAtendimentoCommand(
            Guid.NewGuid(),
            TipoAtendimento.PasseEspiritual,
            45,
            "Limpeza espiritual e desobsessão",
            "Consulente relatou melhora"
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateAtendimentoCommandValidator_Should_Fail_When_Required_Fields_Missing()
    {
        // Arrange
        var validator = new CreateAtendimentoCommandValidator();
        var command = new CreateAtendimentoCommand(
            Guid.Empty,
            (TipoAtendimento)999, // Invalid enum
            0, // Invalid duration
            "", // Empty topics
            ""
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AgendamentoId);
        result.ShouldHaveValidationErrorFor(x => x.Tipo);
        result.ShouldHaveValidationErrorFor(x => x.TempoDuracaoMinutos);
        result.ShouldHaveValidationErrorFor(x => x.TemasAbordados);
    }

    [Fact]
    public void UpdateAtendimentoCommandValidator_Should_Pass_For_Valid_Data()
    {
        // Arrange
        var validator = new UpdateAtendimentoCommandValidator();
        var command = new UpdateAtendimentoCommand(
            Guid.NewGuid(),
            TipoAtendimento.TratamentoEspiritual,
            60,
            "Orientação para caminhos espirituais",
            "Consulta concluída"
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateAtendimentoCommandValidator_Should_Fail_When_Duration_Exceeds_Max()
    {
        // Arrange
        var validator = new UpdateAtendimentoCommandValidator();
        var command = new UpdateAtendimentoCommand(
            Guid.NewGuid(),
            TipoAtendimento.AssistenciaFraterna,
            500, // Exceeds 480
            "Orientação",
            "Obs"
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TempoDuracaoMinutos);
    }

    [Fact]
    public void CreateAcompanhamentoCommandValidator_Should_Pass_For_Valid_Data()
    {
        // Arrange
        var validator = new CreateAcompanhamentoCommandValidator();
        var command = new CreateAcompanhamentoCommand(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Redução da ansiedade e sono regular",
            "Manter banhos de ervas prescritos",
            "Acompanhamento quinzenal"
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateAcompanhamentoCommandValidator_Should_Fail_When_Required_Fields_Empty()
    {
        // Arrange
        var validator = new CreateAcompanhamentoCommandValidator();
        var command = new CreateAcompanhamentoCommand(
            Guid.Empty,
            DateTime.UtcNow.AddDays(10), // Future date
            "",
            "",
            ""
        );

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AtendimentoEspiritualId);
        result.ShouldHaveValidationErrorFor(x => x.DataAcompanhamento);
        result.ShouldHaveValidationErrorFor(x => x.SintomasMelhora);
        result.ShouldHaveValidationErrorFor(x => x.Recomendacoes);
    }

    [Fact]
    public void DeleteAtendimentoCommandValidator_Should_Validate_Id()
    {
        // Arrange
        var validator = new DeleteAtendimentoCommandValidator();

        // Act & Assert
        validator.TestValidate(new DeleteAtendimentoCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.Id);

        validator.TestValidate(new DeleteAtendimentoCommand(Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AtendimentoEspiritualDtoValidator_Should_Pass_For_Valid_Dto()
    {
        // Arrange
        var validator = new AtendimentoEspiritualDtoValidator();
        var dto = new AtendimentoEspiritualDto
        {
            Id = Guid.NewGuid(),
            AgendamentoId = Guid.NewGuid(),
            Tipo = TipoAtendimento.PasseEspiritual,
            TipoDescricao = "Passe Espiritual",
            TempoDuracaoMinutos = 30,
            TemasAbordados = "Equilíbrio energético",
            Observacoes = "Tudo em ordem",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
