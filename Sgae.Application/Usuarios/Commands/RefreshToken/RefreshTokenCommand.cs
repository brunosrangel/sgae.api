using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null
) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("O Refresh Token é obrigatório.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IAppDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.Usuario)
                .ThenInclude(u => u.Sacerdote)
            .Include(r => r.Usuario)
                .ThenInclude(u => u.PastoralRole)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken.Trim(), cancellationToken);

        if (token == null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido ou inexistente.");
        }

        if (token.IsRevoked)
        {
            // Detecção de reutilização de token revogado: revogar todos os tokens do usuário por segurança
            var usuario = await _context.Usuarios
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == token.UsuarioId, cancellationToken);

            if (usuario != null)
            {
                usuario.RevokeAllRefreshTokens(request.IpAddress, "Tentativa de uso de token revogado (possível ataque de replay).");
                await _context.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedAccessException("Sessão revogada por motivos de segurança. Por favor, realize o login novamente.");
        }

        if (token.IsExpired)
        {
            throw new UnauthorizedAccessException("O Refresh Token expirou. Faça login novamente.");
        }

        var user = token.Usuario;
        if (!user.StatusAtivo)
        {
            throw new DomainException("Este usuário está inativo no sistema.");
        }

        // Rotação de Refresh Token: revoga o anterior e cria um novo
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id, request.IpAddress);
        token.Revoke(request.IpAddress, "Rotacionado para novo Refresh Token", newRefreshToken.Token);
        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        user.AddRefreshToken(newRefreshToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            TokenType = "Bearer",
            ExpiresInSeconds = 120 * 60,
            Usuario = new UsuarioDto
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Perfil = user.Perfil,
                PerfilDescricao = user.Perfil.ToString(),
                StatusAtivo = user.StatusAtivo,
                PrimeiroAcesso = user.PrimeiroAcesso,
                UltimoAcesso = user.UltimoAcesso,
                SacerdoteId = user.SacerdoteId,
                SacerdoteNome = user.Sacerdote?.Nome,
                PastoralRoleId = user.PastoralRoleId,
                PastoralRoleNome = user.PastoralRole?.Nome,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
