using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sgae.Application.Abstractions;

namespace Sgae.API.Services;

/// <summary>
/// Implementação do serviço de contexto do usuário atual baseada no HttpContextAccessor.
/// Extrai de forma segura as Claims JWT (ID, E-mail, Nome) e o endereço IP do cliente.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub")
                ?? user?.FindFirstValue("uid");
        }
    }

    public string? UserEmail
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Email)
                ?? user?.FindFirstValue("email")
                ?? user?.FindFirstValue(ClaimTypes.Name);
        }
    }

    public string? UserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Name)
                ?? user?.FindFirstValue("name");
        }
    }

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.ToString().Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string GetUserIdentity()
    {
        if (!IsAuthenticated)
            return "Sistema (Não autenticado)";

        if (!string.IsNullOrWhiteSpace(UserEmail))
            return UserEmail;

        if (!string.IsNullOrWhiteSpace(UserId))
            return $"User:{UserId}";

        if (!string.IsNullOrWhiteSpace(UserName))
            return UserName;

        return "Usuário Autenticado";
    }
}
