using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sgae.Application.Usuarios.Commands.AlterarSenha;
using Sgae.Application.Usuarios.Commands.Login;
using Sgae.Application.Usuarios.Commands.PrimeiroAcesso;
using Sgae.Application.Usuarios.Commands.RefreshToken;
using Sgae.Application.Usuarios.Commands.RegisterUsuario;
using Sgae.Application.Usuarios.Commands.RevokeToken;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Application.Usuarios.Queries.GetCurrentUser;
using Sgae.Domain.Exceptions;

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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Realiza a autenticação com e-mail e senha, retornando o Access Token JWT e o Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var sanitizedEmail = request?.Email?.Trim().ToLowerInvariant() ?? "[null]";

        _logger.LogInformation("SGAE Auth: Tentativa de login recebida para o e-mail '{Email}' a partir do IP '{IpAddress}'.", sanitizedEmail, ipAddress);

        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            _logger.LogWarning("SGAE Auth [400]: Requisição de login com dados incompletos ou nulos.");
            return BadRequest(new { message = "E-mail e senha são obrigatórios para realizar o login.", code = "INVALID_PAYLOAD" });
        }

        try
        {
            var command = new LoginCommand(request.Email, request.Senha, ipAddress);
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [200]: Login efetuado com sucesso para o e-mail '{Email}' (Perfil: {Perfil}).", sanitizedEmail, result.Usuario.Perfil);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [401 - Identity Exception]: Falha de autenticação para o usuário '{Email}'. Motivo: {Reason}", sanitizedEmail, ex.Message);
            return Unauthorized(new { message = ex.Message, code = "INVALID_CREDENTIALS" });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [403 - Domain Rule]: Acesso bloqueado para o usuário '{Email}'. Motivo: {Reason}", sanitizedEmail, ex.Message);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message, code = "ACCOUNT_RESTRICTED" });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [422 - Validation Failure]: Dados de login inválidos para '{Email}'.", sanitizedEmail);
            return UnprocessableEntity(new
            {
                message = "Dados de login inválidos.",
                code = "VALIDATION_ERROR",
                errors = ex.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage })
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogCritical(ex, "SGAE Auth [500 - Database Connection Failure]: Erro de conexão com o banco PostgreSQL durante a tentativa de login de '{Email}'. SqlState: {SqlState}", sanitizedEmail, ex.SqlState);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Falha temporária de comunicação com a base de dados. Por favor, tente novamente em instantes.",
                code = "DATABASE_CONNECTION_ERROR"
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "SGAE Auth [500 - Database Persistence Failure]: Erro ao persistir sessão ou refresh token durante o login de '{Email}'.", sanitizedEmail);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Falha ao registrar a sessão de acesso do usuário.",
                code = "DATABASE_PERSISTENCE_ERROR"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500 - Unhandled Error]: Exceção não tratada durante o login para '{Email}'. Mensagem: {Message}", sanitizedEmail, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Ocorreu um erro interno inesperado ao processar o login.",
                code = "INTERNAL_SERVER_ERROR"
            });
        }
    }

    /// <summary>
    /// Renova o Access Token JWT utilizando um Refresh Token válido.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();

        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("SGAE Auth [400]: Requisição de refresh token sem o token obrigatório.");
            return BadRequest(new { message = "O Refresh Token é obrigatório.", code = "INVALID_REFRESH_TOKEN" });
        }

        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [200]: Token renovado com sucesso para o usuário '{Email}'.", result.Usuario.Email);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [401]: Falha ao renovar token. Motivo: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message, code = "INVALID_OR_EXPIRED_REFRESH_TOKEN" });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [403]: Conta inativa ou bloqueada ao renovar token. Motivo: {Reason}", ex.Message);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message, code = "ACCOUNT_RESTRICTED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro inesperado ao renovar token.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao renovar a sessão do usuário.", code = "REFRESH_TOKEN_ERROR" });
        }
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema SGAE com perfil e vínculos pastorais.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsuarioDto>> Register([FromBody] RegisterUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
            return BadRequest(new { message = "Dados de cadastro não informados.", code = "EMPTY_PAYLOAD" });

        _logger.LogInformation("SGAE Auth: Tentativa de cadastro de novo usuário: '{Email}' (Perfil: {Perfil}).", command.Email, command.Perfil);

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [201]: Usuário cadastrado com sucesso: '{Email}' (Id: {Id}).", result.Email, result.Id);
            return CreatedAtAction(nameof(GetMe), new { }, result);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [400 - Domain Rule]: Regra de negócio violada no cadastro do usuário '{Email}': {Reason}", command?.Email, ex.Message);
            return BadRequest(new { message = ex.Message, code = "REGISTRATION_RULE_VIOLATION" });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [422 - Validation Failure]: Falha de validação ao cadastrar usuário '{Email}'.", command?.Email);
            return UnprocessableEntity(new
            {
                message = "Dados de cadastro inválidos.",
                code = "VALIDATION_ERROR",
                errors = ex.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro inesperado ao cadastrar usuário '{Email}'.", command?.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao processar o cadastro do usuário.", code = "REGISTRATION_ERROR" });
        }
    }

    /// <summary>
    /// Realiza o fluxo de primeiro acesso / definição de senha definitiva para novos usuários.
    /// </summary>
    [HttpPost("primeiro-acesso")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDto>> PrimeiroAcesso([FromBody] PrimeiroAcessoRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Dados de primeiro acesso não informados.", code = "EMPTY_PAYLOAD" });

        var ipAddress = GetClientIpAddress();
        _logger.LogInformation("SGAE Auth: Tentativa de conclusão de primeiro acesso para '{Email}'.", request.Email);

        try
        {
            var command = new PrimeiroAcessoCommand(
                request.Email,
                request.SenhaAtualOuTemporaria,
                request.NovaSenha,
                request.ConfirmacaoNovaSenha,
                ipAddress
            );
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [200]: Primeiro acesso concluído com sucesso para '{Email}'.", request?.Email);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [401]: Credencial temporária inválida no primeiro acesso de '{Email}'.", request?.Email);
            return Unauthorized(new { message = ex.Message, code = "INVALID_CREDENTIALS" });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [400]: Regra violada no primeiro acesso de '{Email}': {Reason}", request?.Email, ex.Message);
            return BadRequest(new { message = ex.Message, code = "DOMAIN_ERROR" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro inesperado no primeiro acesso de '{Email}'.", request?.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao processar o primeiro acesso.", code = "FIRST_ACCESS_ERROR" });
        }
    }

    /// <summary>
    /// Altera a senha do usuário autenticado no sistema (troca de senha).
    /// </summary>
    [Authorize]
    [HttpPost("alterar-senha")]
    [HttpPost("troca-senha")]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequestDto request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("SGAE Auth [401]: Identificador de usuário ausente ou inválido no token JWT.");
            return Unauthorized(new { message = "Identificador de usuário não encontrado no token.", code = "INVALID_USER_TOKEN" });
        }

        var ipAddress = GetClientIpAddress();
        _logger.LogInformation("SGAE Auth: Solicitação de alteração de senha para o usuário Id: '{UserId}'.", userId);

        try
        {
            var command = new AlterarSenhaCommand(userId, request.SenhaAtual, request.NovaSenha, ipAddress);
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [200]: Senha alterada com sucesso para o usuário Id: '{UserId}'.", userId);
            return Ok(new { message = "Senha alterada com sucesso. As sessões ativas anteriores foram revogadas.", code = "PASSWORD_CHANGED" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [401]: Senha atual incorreta ao tentar alterar senha para usuário Id: '{UserId}'.", userId);
            return Unauthorized(new { message = ex.Message, code = "INVALID_CURRENT_PASSWORD" });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [400]: Violação de regra de senha para usuário Id: '{UserId}': {Reason}", userId, ex.Message);
            return BadRequest(new { message = ex.Message, code = "DOMAIN_ERROR" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro inesperado ao alterar senha para usuário Id: '{UserId}'.", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao alterar senha do usuário.", code = "CHANGE_PASSWORD_ERROR" });
        }
    }

    /// <summary>
    /// Revoga o Refresh Token atual e encerra a sessão do usuário (Logout).
    /// </summary>
    [HttpPost("logout")]
    [HttpPost("revoke-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        _logger.LogInformation("SGAE Auth: Solicitação de logout / revogação de token a partir do IP '{IpAddress}'.", ipAddress);

        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "O Refresh Token é obrigatório para logout.", code = "INVALID_PAYLOAD" });
        }

        try
        {
            var command = new RevokeTokenCommand(request.RefreshToken, ipAddress);
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("SGAE Auth [200]: Sessão encerrada e refresh token revogado com sucesso.");
            return Ok(new { message = "Sessão encerrada com sucesso.", code = "LOGOUT_SUCCESS" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro ao revogar token durante logout.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao encerrar a sessão.", code = "LOGOUT_ERROR" });
        }
    }

    /// <summary>
    /// Retorna os dados do perfil do usuário autenticado na sessão atual.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UsuarioDto>> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("SGAE Auth [401]: Tentativa de acesso a /me sem Claim de NameIdentifier válido.");
            return Unauthorized(new { message = "Identificador de usuário não encontrado no token.", code = "INVALID_USER_TOKEN" });
        }

        try
        {
            var result = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "SGAE Auth [404]: Usuário com Id '{UserId}' não encontrado no banco de dados.", userId);
            return NotFound(new { message = "Usuário não localizado.", code = "USER_NOT_FOUND" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE Auth [500]: Erro ao consultar perfil do usuário autenticado Id '{UserId}'.", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao obter os dados do usuário conectado.", code = "GET_ME_ERROR" });
        }
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

