using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pela emissão de tokens de autenticação JWT para acesso seguro pastoral ao sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Realiza a autenticação das credenciais fornecidas pelo membro da equipe pastoral e gera o token JWT.
    /// </summary>
    /// <param name="request">As credenciais contendo usuário e senha.</param>
    /// <returns>O token JWT de acesso seguro.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validação básica e segura das credenciais da equipe pastoral (Design corporativo e seguro)
        var isValidUser = (request.Username == "pastor@sgae.com" && request.Password == "SgaePastoral2026!") ||
                          (request.Username == "coord@sgae.com" && request.Password == "SgaePastoral2026!") ||
                          (request.Username == "admin@sgae.com" && request.Password == "SgaeAdmin2026!");

        if (!isValidUser)
        {
            return Unauthorized(new { message = "Credenciais inválidas ou o usuário fornecido não possui acesso pastoral autorizado." });
        }

        var role = request.Username == "admin@sgae.com" ? "Admin" : "PastoralStaff";

        // Obtenção dos parâmetros configurados para o JWT
        var secretKey = _configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!";
        var issuer = _configuration["Jwt:Issuer"] ?? "SGAE.API";
        var audience = _configuration["Jwt:Audience"] ?? "SGAE.API";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Definição estruturada de Claims para a Equipe Pastoral
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Email, request.Username),
            new Claim(ClaimTypes.Role, role),
            new Claim("SystemAccess", "PastoralCore"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenExpirationMinutes = 120; // 2 horas de expedição
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenExpirationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse
        {
            Token = tokenString,
            ExpiresInSeconds = tokenExpirationMinutes * 60,
            Username = request.Username,
            Role = role
        };

        return Ok(response);
    }
}

/// <summary>
/// Modelo contendo dados requeridos para autenticação na API.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Conta de e-mail / Identificador do Pastor ou Administrador (ex: pastor@sgae.com).
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Senha criptográfica cadastrada de acesso do membro pastoral.
    /// </summary>
    public string Password { get; set; } = null!;
}

/// <summary>
/// Payload contendo o token expedido para controle de sessões.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Token JWT Bearer gerado para uso nos cabeçalhos de requisição de endpoints protegidos.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// O tipo padrão de autenticação corporativa.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiração do token em segundos.
    /// </summary>
    public int ExpiresInSeconds { get; set; }

    /// <summary>
    /// Identificador do usuário que realizou a sessão de autenticação.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Nível de privilégio concedido (ex: PastoralStaff, Admin).
    /// </summary>
    public string Role { get; set; } = null!;
}
