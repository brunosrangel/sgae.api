using System;
using System.Collections.Generic;
using Sgae.Application.Leads.Commands.CreateLead;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Application.Tests.Leads.Commands.CreateLead;

public class CreateLeadCommandValidatorTests
{
    private readonly CreateLeadCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        var command = new CreateLeadCommand
        {
            NomeCompleto = "Bruno Rangel III",
            Telefone = "11987654321",
            Email = "bruno@email.com",
            Cidade = "São Paulo",
            Uf = "SP",
            Origem = OrigemContato.Instagram,
            DataNascimento = new DateTime(1990, 8, 28, 0, 0, 0, DateTimeKind.Utc),
            TradicaoTerreiro = "Candomblé Nagô-Ketu",
            VinculoTradicoes = "Iniciado",
            OrixasNagoKetu = new List<string> { "Èṣù (Exu)", "Ògún (Ogum)" },
            Cep = "01310-100",
            Endereco = "Avenida Paulista",
            Numero = "1000",
            Bairro = "Bela Vista"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithFutureDataNascimento_ShouldFailValidation()
    {
        var command = new CreateLeadCommand
        {
            NomeCompleto = "Bruno Rangel",
            Telefone = "11987654321",
            Email = "bruno@email.com",
            Cidade = "São Paulo",
            Uf = "SP",
            DataNascimento = DateTime.UtcNow.AddYears(10)
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DataNascimento");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFailValidation()
    {
        var command = new CreateLeadCommand
        {
            Nome = "Bruno Rangel",
            Telefone = "11987654321",
            Email = "invalid-email-format",
            Cidade = "São Paulo",
            Estado = "SP"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }
}
