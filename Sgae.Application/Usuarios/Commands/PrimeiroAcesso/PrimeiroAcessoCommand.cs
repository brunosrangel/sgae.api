using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Commands.PrimeiroAcesso;

public record PrimeiroAcessoCommand(
    string Email,
    string SenhaTemporaria,
    string NovaSenha,
    string ConfirmacaoNovaSenha,
    string? IpAddress = null
) : IRequest<AuthResponseDto>;

public class PrimeiroAcessoCommandValidator : AbstractValidator<PrimeiroAcessoCommand>
{
    public PrimeiroAcessoCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(x => x.SenhaTemporaria)
            .NotEmpty().WithMessage("A senha temporária / inicial é obrigatória.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("A nova senha definitiva é obrigatória.")
            .MinimumLength(6).WithMessage("A nova senha deve possuir pelo menos 6 caracteres.")
            .NotEqual(x => x.SenhaTemporaria).WithMessage("A nova senha não pode ser idêntica à senha temporária.");

        RuleFor(x => x.ConfirmacaoNovaSenha)
            .Equal(x => x.NovaSenha).WithMessage("A confirmação da nova senha não confere.");
    }
}

public class PrimeiroAcessoCommandHandler : IRequestHandler<PrimeiroAcessoCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public PrimeiroAcessoCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(PrimeiroAcessoCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .Include(u => u.Sacerdote)
            .Include(u => u.PastoralRole)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (usuario == null || !_passwordHasher.VerifyPassword(request.SenhaTemporaria, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciais de primeiro acesso inválidas.");
        }

        if (!usuario.StatusAtivo)
        {
            throw new DomainException("Este usuário está inativo no sistema.");
        }

        var newHash = _passwordHasher.HashPassword(request.NovaSenha);
        usuario.CompletePrimeiroAcesso(newHash);
        usuario.RevokeAllRefreshTokens(request.IpAddress, "Primeiro acesso concluído.");

        var accessToken = _tokenService.GenerateAccessToken(usuario);
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
            ExpiresInSeconds = 120 * 60,
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
