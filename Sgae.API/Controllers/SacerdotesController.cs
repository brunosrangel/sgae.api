using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Sacerdotes.DTOs;
using Sgae.Application.Sacerdotes.Queries.GetSacerdotes;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pela gestão e consulta dos Sacerdotes Escalados / Corpo Litúrgico.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SacerdotesController : ControllerBase
{
    private readonly ISender _sender;

    public SacerdotesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista os Sacerdotes Escalados / Corpo Litúrgico cadastrados e ativos no banco de dados.
    /// </summary>
    /// <param name="apenasAtivos">Se verdadeiro, filtra apenas sacerdotes em atividade pastoral (padrão true).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de Sacerdotes cadastrados.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SacerdoteDto>>> GetSacerdotes(
        [FromQuery] bool? apenasAtivos = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetSacerdotesQuery { ApenasAtivos = apenasAtivos }, cancellationToken);
        return Ok(result);
    }
}
