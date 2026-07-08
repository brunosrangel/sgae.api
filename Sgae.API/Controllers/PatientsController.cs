using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
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
/// Controller responsável pela gestão e operações CRUD de Pacientes (Consulentes/Leads).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly ISender _sender;

    public PatientsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Cadastra um novo Paciente (Consulente).
    /// </summary>
    /// <param name="command">Dados do paciente para cadastro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>ID do Paciente gerado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeadCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um determinado Paciente pelo ID, incluindo seu perfil correspondente se cadastrado.
    /// </summary>
    /// <param name="id">ID do Paciente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os dados do Paciente.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeadByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Paciente de ID '{id}' não foi localizado no sistema." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Lista os Pacientes de forma paginada com parâmetros de pesquisa e filtros opcionais.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão 1).</param>
    /// <param name="pageSize">Quantidade de registros por página (padrão 10).</param>
    /// <param name="searchTerm">Filtro de pesquisa por nome/telefone/email.</param>
    /// <param name="dataInicio">Início do intervalo de data de captação.</param>
    /// <param name="dataFim">Fim do intervalo de data de captação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado dos pacientes.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LeadDto>>> GetPaged(
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

    /// <summary>
    /// Atualiza as informações de um Paciente existente.
    /// </summary>
    /// <param name="id">ID do Paciente.</param>
    /// <param name="command">Dados atualizados do paciente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Status sem conteúdo em caso de sucesso.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status24NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateLeadInputModel command,
        CancellationToken cancellationToken)
    {
        try
        {
            var updateCommand = new UpdateLeadCommand(
                id,
                command.Nome,
                command.Telefone,
                command.Email,
                command.Cidade,
                command.Estado,
                command.ProblemaPrincipal
            );

            await _sender.Send(updateCommand, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("não localizado"))
            {
                return NotFound(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Exclui de forma lógica (Soft Delete) um Paciente do sistema.
    /// </summary>
    /// <param name="id">ID do Paciente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Status sem conteúdo em caso de sucesso.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status24NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
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
            return NotFound(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Modelo de entrada simplificado para atualização de dados de um Paciente.
/// </summary>
public class UpdateLeadInputModel
{
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Cidade { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public string ProblemaPrincipal { get; set; } = null!;
}
