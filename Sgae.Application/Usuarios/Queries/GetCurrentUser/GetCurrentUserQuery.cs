using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Exceptions;

namespace Sgae.Application.Usuarios.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UsuarioId) : IRequest<UsuarioDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UsuarioDto>
{
    private readonly IAppDbContext _context;

    public GetCurrentUserQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Sacerdote)
            .Include(u => u.PastoralRole)
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);

        if (usuario == null)
        {
            throw new NotFoundException("Usuario", request.UsuarioId);
        }

        return new UsuarioDto
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
        };
    }
}
