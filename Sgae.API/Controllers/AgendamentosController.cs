using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sgae.Application.Agendamentos.Commands.CreateAgendamento;
using Sgae.Application.Agendamentos.DTOs;
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
    public async Task<IActionResult> CreateAgendamento(
        [FromBody] CreateAgendamentoCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetAgendamentos), new { id }, id);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
