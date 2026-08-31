using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Agendamentos.Commands.CreateAgendamento;
using Sgae.Application.Agendamentos.Commands.DeleteAgendamento;
using Sgae.Application.Agendamentos.Commands.UpdateAgendamento;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Application.Agendamentos.Queries.GetAgendamentoById;
using Sgae.Application.Agendamentos.Queries.GetAgendamentosWithFilters;
using Sgae.Domain.Enums;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints do Módulo 2 - Agendamentos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly ISender _sender;

    public AgendamentosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra um novo agendamento de consulta para um determinado Consulente/Lead.
    /// </summary>
    /// <param name="command">Dados do agendamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>ID do agendamento criado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAgendamento(
        [FromBody] CreateAgendamentoCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAgendamentoById), new { id }, new { id });
    }

    /// <summary>
    /// Atualiza os dados de um agendamento existente.
    /// </summary>
    /// <param name="id">ID do agendamento.</param>
    /// <param name="command">Dados atualizados do agendamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Sem conteúdo em caso de sucesso.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAgendamento(
        Guid id,
        [FromBody] UpdateAgendamentoCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        try
        {
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("não foi localizado"))
                return NotFound(new { error = ex.Message });
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove de forma lógica um agendamento do sistema.
    /// </summary>
    /// <param name="id">ID do agendamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Sem conteúdo em caso de sucesso.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAgendamento(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new DeleteAgendamentoCommand(id), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um agendamento específico pelo seu identificador único.
    /// </summary>
    /// <param name="id">ID do agendamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do agendamento.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendamentoDto>> GetAgendamentoById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAgendamentoByIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Agendamento com ID '{id}' não foi encontrado." });

        return Ok(result);
    }

    /// <summary>
    /// Obtém a listagem filtrada de agendamentos de consulta espiritual do sistema.
    /// </summary>
    /// <param name="dataInicio">Início do intervalo de data do agendamento.</param>
    /// <param name="dataFim">Fim do intervalo de data do agendamento.</param>
    /// <param name="modalidade">Filtro de Modalidade (Presencial, Online).</param>
    /// <param name="status">Filtro de Status (Pendente, Confirmado, Realizado, Cancelado).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de agendamentos DTO.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgendamentoDto>>> GetAgendamentos(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] ModalidadeAtendimento? modalidade,
        [FromQuery] StatusAgendamento? status,
        CancellationToken cancellationToken)
    {
        var query = new GetAgendamentosWithFiltersQuery
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            Modalidade = modalidade,
            Status = status
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
