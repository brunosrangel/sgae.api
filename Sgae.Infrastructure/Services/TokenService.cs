using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;

namespace Sgae.Infrastructure.Services;

/// <summary>
/// Serviço de emissão, decodificação e renovação de tokens de segurança JWT e Refresh Tokens do SGAE.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        var secretKey = _configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!";
        var issuer = _configuration["Jwt:Issuer"] ?? "SGAE.API";
        var audience = _configuration["Jwt:Audience"] ?? "SGAE.API";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("Perfil", usuario.Perfil.ToString()),
            new Claim("PrimeiroAcesso", usuario.PrimeiroAcesso.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.SacerdoteId.HasValue)
        {
            claims.Add(new Claim("SacerdoteId", usuario.SacerdoteId.Value.ToString()));
        }

        if (usuario.PastoralRoleId.HasValue)
        {
            claims.Add(new Claim("PastoralRoleId", usuario.PastoralRoleId.Value.ToString()));
        }

        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 120; // 2 horas padrão
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid usuarioId, string? ipAddress, int daysToExpire = 7)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenString = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var expiresAt = DateTime.UtcNow.AddDays(daysToExpire);
        return new RefreshToken(tokenString, usuarioId, expiresAt, ipAddress);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "SuperSecretRobustKeyForSgaeSystem2026ValidationAndSecuritySignatures!";
        var issuer = _configuration["Jwt:Issuer"] ?? "SGAE.API";
        var audience = _configuration["Jwt:Audience"] ?? "SGAE.API";

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false // Permite ler claims mesmo após a expiração natural
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
