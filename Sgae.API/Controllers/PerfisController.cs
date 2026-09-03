using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Perfis.Commands.CreatePerfilConsulente;
using Sgae.Application.Perfis.DTOs;
using Sgae.Application.Perfis.Queries.GetPerfilConsulenteByLeadId;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável por expor os endpoints do Módulo 3 - Perfil do Consulente.
/// </summary>
[Authorize(Roles = "Admin,Secretaria,Sacerdote")]
[ApiController]
[Route("api/[controller]")]
public class PerfisController : ControllerBase
{
    private readonly ISender _sender;

    public PerfisController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Cadastra ou atualiza o perfil demográfico de um determinado Consulente (LeadId).
    /// </summary>
    /// <param name="command">Informações demográficas e de contexto do consulente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O identificador único do perfil gerado/atualizado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertPerfil(
        [FromBody] CreatePerfilConsulenteCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return Ok(new { id, message = "Perfil demográfico estabelecido com sucesso." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém o perfil demográfico cadastrado de um Consulente pelo seu Lead ID.
    /// </summary>
    /// <param name="leadId">Identificador único do Consulente (Lead).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O perfil demográfico do consulente se existir.</returns>
    [HttpGet("lead/{leadId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilConsulenteDto>> GetPerfilByLeadId(
        Guid leadId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPerfilConsulenteByLeadIdQuery(leadId), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Perfil epidemiológico/demográfico não localizado para o Consulente de ID '{leadId}'." });
        }
        return Ok(result);
    }
}
