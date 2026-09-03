namespace Sgae.Application.Abstractions;

/// <summary>
/// Provedor para obtenção de contexto do usuário autenticado atual na requisição HTTP (Identidade, E-mail, IP e Identificador).
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserEmail { get; }
    string? UserName { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
    string GetUserIdentity();
}
