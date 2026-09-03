using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Commands.AlterarSenha;

public record AlterarSenhaCommand(
    Guid UsuarioId,
    string SenhaAtual,
    string NovaSenha,
    string? IpAddress = null
) : IRequest<bool>;

public class AlterarSenhaCommandValidator : AbstractValidator<AlterarSenhaCommand>
{
    public AlterarSenhaCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("O identificador do usuário é obrigatório.");

        RuleFor(x => x.SenhaAtual)
            .NotEmpty().WithMessage("A senha atual é obrigatória.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("A nova senha é obrigatória.")
            .MinimumLength(6).WithMessage("A nova senha deve possuir pelo menos 6 caracteres.")
            .NotEqual(x => x.SenhaAtual).WithMessage("A nova senha não pode ser idêntica à senha atual.");
    }
}

public class AlterarSenhaCommandHandler : IRequestHandler<AlterarSenhaCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AlterarSenhaCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(AlterarSenhaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);

        if (usuario == null)
        {
            throw new NotFoundException("Usuario", request.UsuarioId);
        }

        if (!_passwordHasher.VerifyPassword(request.SenhaAtual, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("A senha atual informada está incorreta.");
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NovaSenha);
        usuario.UpdatePassword(newPasswordHash);

        // Revoga todos os refresh tokens anteriores para forçar nova autenticação com a nova senha
        usuario.RevokeAllRefreshTokens(request.IpAddress, "Senha alterada pelo usuário.");

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
