using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Commands.Login;

public record LoginCommand(
    string Email,
    string Senha,
    string? IpAddress = null
) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .Include(u => u.Sacerdote)
            .Include(u => u.PastoralRole)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (usuario == null || !_passwordHasher.VerifyPassword(request.Senha, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciais de acesso inválidas ou usuário não cadastrado.");
        }

        if (!usuario.StatusAtivo)
        {
            throw new DomainException("Este usuário está inativo no sistema. Entre em contato com a administração.");
        }

        // Gera novo access token
        var accessToken = _tokenService.GenerateAccessToken(usuario);

        // Gera novo refresh token e persiste
        var refreshToken = _tokenService.GenerateRefreshToken(usuario.Id, request.IpAddress);
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        usuario.AddRefreshToken(refreshToken);
        usuario.RecordLogin();

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            TokenType = "Bearer",
            ExpiresInSeconds = 120 * 60, // 2 horas
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                PerfilDescricao = usuario.Perfil.ToString(),
                StatusAtivo = usuario.StatusAtivo,
                PrimeiroAcesso = usuario.PrimeiroAcesso,
                UltimoAcesso = usuario.UltimoAcesso,
                SacerdoteId = usuario.SacerdoteId,
                SacerdoteNome = usuario.Sacerdote?.Nome,
                PastoralRoleId = usuario.PastoralRoleId,
                PastoralRoleNome = usuario.PastoralRole?.Nome,
                CreatedAt = usuario.CreatedAt
            }
        };
    }
}
