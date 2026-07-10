using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Atendimentos.Commands.CreateAtendimento;
using Sgae.Application.Atendimentos.Commands.UpdateAtendimento;
using Sgae.Application.Atendimentos.Commands.DeleteAtendimento;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Atendimentos.Queries.GetAllAtendimentos;
using Sgae.Application.Atendimentos.Queries.GetAtendimentoById;
using Sgae.Application.Common.Models;
using Sgae.Domain.Enums;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pela gestão e operações CRUD de Consultas Espirituais (Atendimentos Espirituais).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SpiritualConsultationsController : ControllerBase
{
    private readonly ISender _sender;

    public SpiritualConsultationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra um novo Atendimento Espiritual.
    /// </summary>
    /// <param name="command">Dados do atendimento espiritual para registro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>ID do Atendimento Espiritual gerado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAtendimentoCommand command,
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um determinado Atendimento Espiritual pelo ID.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os dados do Atendimento Espiritual.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtendimentoEspiritualDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAtendimentoByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Atendimento Espiritual de ID '{id}' não localizado." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Lista os Atendimentos Espirituais de forma paginada.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão 1).</param>
    /// <param name="pageSize">Quantidade máxima de registros por página (padrão 10).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado paginado dos atendimentos espirituais.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AtendimentoEspiritualDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAtendimentosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza as informações de um Atendimento Espiritual existente.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="command">Dados atualizados do atendimento espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Status sem conteúdo em caso de sucesso.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAtendimentoInputModel command,
        CancellationToken cancellationToken)
    {
        try
        {
            var updateCommand = new UpdateAtendimentoCommand(
                id,
                command.Tipo,
                command.TempoDuracaoMinutos,
                command.TemasAbordados,
                command.Observacoes
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
    /// Exclui de forma lógica (Soft Delete) um Atendimento Espiritual do sistema.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Status sem conteúdo em caso de sucesso.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new DeleteAtendimentoCommand(id), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Modelo de entrada simplificado para atualização de dados de um Atendimento Espiritual.
/// </summary>
public class UpdateAtendimentoInputModel
{
    public TipoAtendimento Tipo { get; set; }
    public int TempoDuracaoMinutos { get; set; }
    public string TemasAbordados { get; set; } = null!;
    public string Observacoes { get; set; } = null!;
}
