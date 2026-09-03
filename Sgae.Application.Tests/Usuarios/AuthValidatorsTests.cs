using FluentAssertions;
using Sgae.Application.Usuarios.Commands.AlterarSenha;
using Sgae.Application.Usuarios.Commands.Login;
using Sgae.Application.Usuarios.Commands.PrimeiroAcesso;
using Sgae.Application.Usuarios.Commands.RefreshToken;
using Sgae.Application.Usuarios.Commands.RegisterUsuario;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Application.Tests.Usuarios;

public class AuthValidatorsTests
{
    [Fact]
    public void LoginCommandValidator_ComCamposVazios_DeveFalhar()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("", "");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Fact]
    public void LoginCommandValidator_ComEmailValidoESenha_DevePassar()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("pastor@sgae.com", "Sgae123!");

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenCommandValidator_ComTokenVazio_DeveFalhar()
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public void RegisterUsuarioCommandValidator_ComDadosValidos_DevePassar()
    {
        var validator = new RegisterUsuarioCommandValidator();
        var command = new RegisterUsuarioCommand(
            "Novo Sacerdote",
            "sacerdote2@sgae.com",
            "SenhaForte2026!",
            PerfilUsuario.Sacerdote
        );

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AlterarSenhaCommandValidator_ComSenhasIguais_DeveFalhar()
    {
        var validator = new AlterarSenhaCommandValidator();
        var command = new AlterarSenhaCommand(Guid.NewGuid(), "MesmaSenha123", "MesmaSenha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NovaSenha");
    }

    [Fact]
    public void PrimeiroAcessoCommandValidator_ComConfirmacaoDivergente_DeveFalhar()
    {
        var validator = new PrimeiroAcessoCommandValidator();
        var command = new PrimeiroAcessoCommand(
            "user@sgae.com",
            "temp123",
            "NovaSenhaForte123",
            "DiferenteSenha123"
        );

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmacaoNovaSenha");
    }
}
