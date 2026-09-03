using MediatR;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Domain.Enums;

namespace Sgae.Application.Usuarios.Queries.GetUsuarios;

public record GetUsuariosQuery(
    bool? ApenasAtivos = null,
    PerfilUsuario? Perfil = null
) : IRequest<List<UsuarioDto>>;

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, List<UsuarioDto>>
{
    private readonly IAppDbContext _context;

    public GetUsuariosQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Usuarios
            .Include(u => u.Sacerdote)
            .Include(u => u.PastoralRole)
            .AsNoTracking();

        if (request.ApenasAtivos.HasValue && request.ApenasAtivos.Value)
        {
            query = query.Where(u => u.StatusAtivo);
        }

        if (request.Perfil.HasValue)
        {
            query = query.Where(u => u.Perfil == request.Perfil.Value);
        }

        var list = await query
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);

        return list.Select(u => new UsuarioDto
        {
            Id = usuarioId(u),
            Nome = u.Nome,
            Email = u.Email,
            Perfil = u.Perfil,
            PerfilDescricao = u.Perfil.ToString(),
            StatusAtivo = u.StatusAtivo,
            PrimeiroAcesso = u.PrimeiroAcesso,
            UltimoAcesso = u.UltimoAcesso,
            SacerdoteId = u.SacerdoteId,
            SacerdoteNome = u.Sacerdote?.Nome,
            PastoralRoleId = u.PastoralRoleId,
            PastoralRoleNome = u.PastoralRole?.Nome,
            CreatedAt = u.CreatedAt
        }).ToList();

        static Guid usuarioId(Domain.Entities.Usuario u) => u.Id;
    }
}
