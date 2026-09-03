using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Common.Models;
using Sgae.Application.Leads.Commands.CreateLead;
using Sgae.Application.Leads.Commands.UpdateLead;
using Sgae.Application.Leads.Commands.DeleteLead;
using Sgae.Application.Leads.DTOs;
using Sgae.Application.Leads.Queries.GetLeadById;
using Sgae.Application.Leads.Queries.GetLeadsWithPagination;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints do Módulo de Captação (Leads).
/// </summary>
[Authorize(Roles = "Admin,Secretaria,Sacerdote")]
[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ISender _sender;

    public LeadsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Cadastra um novo consulente/lead inicial no sistema de captação.
    /// Suporta a nova estrutura completa de dados com vínculos religiosos, orixás, endereço e histórico.
    /// </summary>
    /// <param name="command">Dados do consulente para cadastro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>ID do Consulente gerado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLead(
        [FromBody] CreateLeadCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetLeadById), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um determinado Consulente (Lead) pelo ID, incluindo seu correspondente perfil, histórico e vínculos.
    /// </summary>
    /// <param name="id">ID do consulente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os dados completos do consulente (Lead).</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDto>> GetLeadById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeadByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Consulente de ID '{id}' não foi localizado no sistema." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Atualiza os dados de um Lead/Consulente existente.
    /// </summary>
    /// <param name="id">ID do Lead.</param>
    /// <param name="command">Dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLead(
        Guid id,
        [FromBody] UpdateLeadCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            command.Id = id;
        }

        try
        {
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um Lead do sistema (Soft Delete).
    /// </summary>
    /// <param name="id">ID do Lead.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLead(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new DeleteLeadCommand(id), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista os Consulentes (Leads) de forma paginada e com múltiplos campos de filtros.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão 1).</param>
    /// <param name="pageSize">Quantidade de registros por página (padrão 10).</param>
    /// <param name="searchTerm">Filtro de pesquisa por nome/telefone/email/código/cidade.</param>
    /// <param name="dataInicio">Início do intervalo de data de captação.</param>
    /// <param name="dataFim">Fim do intervalo de data de captação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado dos consulentes cadastrados.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LeadDto>>> GetLeads(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeadsWithPaginationQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            DataInicio = dataInicio,
            DataFim = dataFim
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
