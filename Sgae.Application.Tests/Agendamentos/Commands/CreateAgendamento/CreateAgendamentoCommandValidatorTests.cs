using System;
using Sgae.Application.Agendamentos.Commands.CreateAgendamento;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Application.Tests.Agendamentos.Commands.CreateAgendamento;

public class CreateAgendamentoCommandValidatorTests
{
    private readonly CreateAgendamentoCommandValidator _validator = new();

    [Fact]
    public void Validate_WithIncomingUserPayload_ShouldPassValidation()
    {
        var command = new CreateAgendamentoCommand
        {
            LeadId = Guid.NewGuid(),
            Data = "2026-08-28",
            Horario = "11:00",
            Sacerdote = "Babalorixa Sidnei T' Sango",
            TipoConsulta = "Jogo de Búzios",
            Valor = 250,
            Status = "Pendente",
            FormaPagamento = "Cartão",
            Pago = false,
            Observacoes = "",
            WhatsappConfirmacaoDisparada = false,
            ConfigLembrete = "Não Configurado",
            FrequenciaLembrete = "Nenhum",
            LeadNome = "Bruno Rangel III",
            DataHora = new DateTime(2026, 8, 28, 11, 0, 0, DateTimeKind.Utc),
            Modalidade = ModalidadeAtendimento.Presencial,
            LeadTelefone = "",
            Atendimento = null
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyLeadId_ShouldFailValidation()
    {
        var command = new CreateAgendamentoCommand
        {
            LeadId = Guid.Empty,
            Data = "2026-08-28",
            Horario = "11:00",
            Valor = 250
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LeadId");
    }

    [Fact]
    public void Validate_WithNegativeValor_ShouldFailValidation()
    {
        var command = new CreateAgendamentoCommand
        {
            LeadId = Guid.NewGuid(),
            Data = "2026-08-28",
            Horario = "11:00",
            Valor = -50
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Valor");
    }
}
