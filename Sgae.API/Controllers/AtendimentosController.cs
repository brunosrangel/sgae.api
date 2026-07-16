using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;
using Sgae.Application.Atendimentos.Commands.CreateAtendimento;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Atendimentos.Queries.GetAcompanhamentosByAtendimentoId;
using Sgae.Application.Atendimentos.Queries.GetAllAtendimentos;
using Sgae.Application.Atendimentos.Queries.GetAtendimentoById;
using Sgae.Application.Common.Models;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável por expor os endpoints de Gestão de Atendimento Espiritual (Etapa 4) e Acompanhamento de evolução.
/// </summary>
[Authorize(Roles = "PastoralStaff,Admin")]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AtendimentosPolicy")]
public class AtendimentosController : ControllerBase
{
    private readonly ISender _sender;

    public AtendimentosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra um novo atendimento espiritual e atualiza as informações do agendamento correspondente.
    /// </summary>
    /// <param name="command">Payload com os dados do atendimento.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>ID do atendimento espiritual registrado.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAtendimento(
        [FromBody] CreateAtendimentoCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetAtendimentoById), new { id }, id);
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
    /// Recupera a listagem paginada de atendimentos espirituais registrados via Dapper com alta performance.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão 1).</param>
    /// <param name="pageSize">Quantidade máxima de atendimentos por página (padrão 10).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta paginada de atendimentos espirituais.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AtendimentoEspiritualDto>>> GetAllAtendimentos(
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
    /// Obtém os detalhes de um atendimento espiritual específico pelo ID via Dapper.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os dados do Atendimento Espiritual.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtendimentoEspiritualDto>> GetAtendimentoById(
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
    /// Registra uma nova sessão de acompanhamento (monitoramento de evolução) para um atendimento espiritual ativo.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual (âncora).</param>
    /// <param name="input">Informações do progresso e novas orientações do consulente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O ID do acompanhamento registrado.</returns>
    [HttpPost("{id:guid}/acompanhamentos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAcompanhamento(
        Guid id,
        [FromBody] CreateAcompanhamentoInputModel input,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAcompanhamentoCommand(
                id,
                input.DataAcompanhamento,
                input.SintomasMelhora,
                input.Recomendacoes,
                input.Observacoes
            );

            var acompanhamentoId = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(
                nameof(GetAcompanhamentosByAtendimentoId),
                new { id },
                acompanhamentoId
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todas as sessões de acompanhamento/monitoramento registradas para um Atendimento Espiritual via Dapper.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de acompanhamentos.</returns>
    [HttpGet("{id:guid}/acompanhamentos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AcompanhamentoDto>>> GetAcompanhamentosByAtendimentoId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAcompanhamentosByAtendimentoIdQuery(id), cancellationToken);
        return Ok(result);
    }
}

/// <summary>
/// Modelo de entrada simplificado para criação de Acompanhamento ancorado na rota de Atendimento.
/// </summary>
public class CreateAcompanhamentoInputModel
{
    public DateTime DataAcompanhamento { get; set; }
    public string SintomasMelhora { get; set; } = null!;
    public string Recomendacoes { get; set; } = null!;
    public string Observacoes { get; set; } = null!;
}
