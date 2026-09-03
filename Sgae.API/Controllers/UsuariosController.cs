using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Usuarios.Commands.RegisterUsuario;
using Sgae.Application.Usuarios.DTOs;
using Sgae.Application.Usuarios.Queries.GetCurrentUser;
using Sgae.Application.Usuarios.Queries.GetUsuarios;
using Sgae.Domain.Enums;
using Sgae.Domain.Exceptions;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller para administração e gerenciamento de usuários do SGAE.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Secretaria")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public UsuariosController(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Lista todos os usuários cadastrados com suporte a filtros por status e perfil.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioDto>>> GetAll(
        [FromQuery] bool? apenasAtivos,
        [FromQuery] PerfilUsuario? perfil,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsuariosQuery(apenasAtivos, perfil), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de um usuário por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo usuário no sistema (Requer privilégios de Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AuthPolicy")]
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] RegisterUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Ativa ou desativa um usuário no sistema (Requer privilégios de Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        usuario.SetStatus(request.StatusAtivo);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = $"Status do usuário atualizado para {(request.StatusAtivo ? "Ativo" : "Inativo")}." });
    }
}

public class UpdateStatusRequest
{
    public bool StatusAtivo { get; set; }
}
