using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade de Usuário do sistema SGAE com suporte a autenticação segura, perfis de acesso (RBAC),
/// primeiro acesso, vínculos pastorais e controle de tokens de renovação.
/// </summary>
public class Usuario : BaseEntity
{
    private readonly List<RefreshToken> _refreshTokens = new();

    private Usuario() { }

    public Usuario(
        string nome,
        string email,
        string passwordHash,
        PerfilUsuario perfil,
        Guid? sacerdoteId = null,
        Guid? pastoralRoleId = null,
        bool primeiroAcesso = false)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("O hash de senha é obrigatório.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Perfil = perfil;
        StatusAtivo = true;
        PrimeiroAcesso = primeiroAcesso;
        SacerdoteId = sacerdoteId;
        PastoralRoleId = pastoralRoleId;
    }

    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public PerfilUsuario Perfil { get; private set; }
    public bool StatusAtivo { get; private set; } = true;
    public bool PrimeiroAcesso { get; private set; } = false;
    public DateTime? UltimoAcesso { get; private set; }

    public Guid? SacerdoteId { get; private set; }
    public virtual Sacerdote? Sacerdote { get; private set; }

    public Guid? PastoralRoleId { get; private set; }
    public virtual PastoralRole? PastoralRole { get; private set; }

    public virtual IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("O novo hash de senha é obrigatório.");

        PasswordHash = newPasswordHash;
        PrimeiroAcesso = false;
        RegisterUpdate();
    }

    public void CompletePrimeiroAcesso(string newPasswordHash)
    {
        UpdatePassword(newPasswordHash);
        PrimeiroAcesso = false;
    }

    public void UpdateProfile(
        string nome,
        PerfilUsuario perfil,
        Guid? sacerdoteId = null,
        Guid? pastoralRoleId = null,
        bool statusAtivo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuário é obrigatório.");

        Nome = nome.Trim();
        Perfil = perfil;
        SacerdoteId = sacerdoteId;
        PastoralRoleId = pastoralRoleId;
        StatusAtivo = statusAtivo;
        RegisterUpdate();
    }

    public void SetStatus(bool statusAtivo)
    {
        StatusAtivo = statusAtivo;
        RegisterUpdate();
    }

    public void RecordLogin()
    {
        UltimoAcesso = DateTime.UtcNow;
        RegisterUpdate();
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        _refreshTokens.Add(refreshToken);
    }

    public void RevokeAllRefreshTokens(string? ipAddress, string? reason)
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(ipAddress, reason);
        }
        RegisterUpdate();
    }
}
