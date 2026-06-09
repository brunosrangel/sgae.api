using System;
using System.Collections.Generic;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Queries.GetAcompanhamentosByAtendimentoId;

/// <summary>
/// Consulta para obter todos os Acompanhamentos vinculados a um Atendimento Espiritual.
/// </summary>
public record GetAcompanhamentosByAtendimentoIdQuery(Guid AtendimentoEspiritualId) : IQuery<List<AcompanhamentoDto>>;
