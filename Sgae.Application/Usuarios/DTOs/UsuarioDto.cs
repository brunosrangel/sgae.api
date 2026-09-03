using Sgae.Domain.Enums;

namespace Sgae.Application.Usuarios.DTOs;

/// <summary>
/// DTO representando os dados públicos e administrativos de um Usuário do sistema SGAE.
/// </summary>
public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public PerfilUsuario Perfil { get; set; }
    public string PerfilDescricao { get; set; } = null!;
    public bool StatusAtivo { get; set; }
    public bool PrimeiroAcesso { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public Guid? SacerdoteId { get; set; }
    public string? SacerdoteNome { get; set; }
    public Guid? PastoralRoleId { get; set; }
    public string? PastoralRoleNome { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO contendo o resultado da autenticação com tokens e perfil do usuário logado.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}
