using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;

namespace Sgae.Application.Usuarios.Commands.RevokeToken;

public record RevokeTokenCommand(
    string RefreshToken,
    string? IpAddress = null
) : IRequest<bool>;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("O Refresh Token é obrigatório.");
    }
}

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IAppDbContext _context;

    public RevokeTokenCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken.Trim(), cancellationToken);

        if (token == null || token.IsRevoked)
        {
            return true;
        }

        token.Revoke(request.IpAddress, "Revogado a pedido do usuário (Logout).");
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
