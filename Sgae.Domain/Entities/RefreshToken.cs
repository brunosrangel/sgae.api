using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando um Refresh Token criptográfico para renovação transparente e segura de sessões JWT.
/// </summary>
public class RefreshToken : BaseEntity
{
    private RefreshToken() { }

    public RefreshToken(
        string token,
        Guid usuarioId,
        DateTime expiresAt,
        string? createdByIp = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("O token criptográfico é obrigatório.");

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("O Refresh Token deve estar vinculado a um usuário válido.");

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("A data de expiração do Refresh Token deve estar no futuro.");

        Token = token.Trim();
        UsuarioId = usuarioId;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        IsRevoked = false;
    }

    public string Token { get; private set; } = null!;
    public Guid UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Revoga o token de atualização impedindo novos ciclos de renovação.
    /// </summary>
    public void Revoke(string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
        RegisterUpdate();
    }
}
