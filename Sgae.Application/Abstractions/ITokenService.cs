using System.Security.Claims;
using Sgae.Domain.Entities;

namespace Sgae.Application.Abstractions;

/// <summary>
/// Contrato para emissão, renovação e validação de tokens JWT e Refresh Tokens do SGAE.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera o Access Token JWT assinado contendo as Claims do usuário autenticado.
    /// </summary>
    string GenerateAccessToken(Usuario usuario);

    /// <summary>
    /// Gera um Refresh Token criptográfico com validade estendida.
    /// </summary>
    RefreshToken GenerateRefreshToken(Guid usuarioId, string? ipAddress, int daysToExpire = 7);

    /// <summary>
    /// Extrai o ClaimsPrincipal a partir de um token JWT expirado para validação em rotações de sessão.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
