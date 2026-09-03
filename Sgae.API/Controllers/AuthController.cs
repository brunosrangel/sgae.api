using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sgae.Application.Usuarios.Commands.AlterarSenha;
using Sgae.Application.Usuarios.Commands.Login;
using Sgae.Application.Usuarios.Commands.PrimeiroAcesso;
using Sgae.Application.Usuarios.Commands.RefreshToken;
using Sgae.Application.Usuarios.Commands.RegisterUsuario;
using Sgae.Application.Usuarios.Commands.RevokeToken;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Application.Usuarios.Queries.GetCurrentUser;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pelos fluxos de autenticação, emissão e renovação de tokens JWT (Refresh Tokens),
/// cadastro de usuários, primeiro acesso e alteração de senha no SGAE.
/// Protegido contra ataques de força bruta através da política de Rate Limiting 'AuthPolicy'.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Realiza a autenticação com e-mail e senha, retornando o Access Token JWT e o Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var command = new LoginCommand(request.Email, request.Senha, ipAddress);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Renova o Access Token JWT utilizando um Refresh Token válido.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema SGAE com perfil e vínculos pastorais.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UsuarioDto>> Register([FromBody] RegisterUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMe), new { }, result);
    }

    /// <summary>
    /// Realiza o fluxo de primeiro acesso / definição de senha definitiva para novos usuários.
    /// </summary>
    [HttpPost("primeiro-acesso")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> PrimeiroAcesso([FromBody] PrimeiroAcessoRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var command = new PrimeiroAcessoCommand(
            request.Email,
            request.SenhaAtualOuTemporaria,
            request.NovaSenha,
            request.ConfirmacaoNovaSenha,
            ipAddress
        );
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Altera a senha do usuário autenticado no sistema (troca de senha).
    /// </summary>
    [Authorize]
    [HttpPost("alterar-senha")]
    [HttpPost("troca-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequestDto request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Identificador de usuário não encontrado no token." });
        }

        var ipAddress = GetClientIpAddress();
        var command = new AlterarSenhaCommand(userId, request.SenhaAtual, request.NovaSenha, ipAddress);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Senha alterada com sucesso. As sessões ativas anteriores foram revogadas." });
    }

    /// <summary>
    /// Revoga o Refresh Token atual e encerra a sessão do usuário (Logout).
    /// </summary>
    [HttpPost("logout")]
    [HttpPost("revoke-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var command = new RevokeTokenCommand(request.RefreshToken, ipAddress);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Sessão encerrada com sucesso." });
    }

    /// <summary>
    /// Retorna os dados do perfil do usuário autenticado na sessão atual.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioDto>> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Identificador de usuário não encontrado no token." });
        }

        var result = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(result);
    }

    private string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
        {
            return forwardedHeader.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Senha { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
}

public class PrimeiroAcessoRequestDto
{
    public string Email { get; set; } = null!;
    public string SenhaAtualOuTemporaria { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
    public string ConfirmacaoNovaSenha { get; set; } = null!;
}

public class AlterarSenhaRequestDto
{
    public string SenhaAtual { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
}
