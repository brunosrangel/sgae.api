using System;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Queries.GetAtendimentoById;

/// <summary>
/// Consulta para obter um Atendimento Espiritual por ID.
/// </summary>
public record GetAtendimentoByIdQuery(Guid Id) : IQuery<AtendimentoEspiritualDto?>;
