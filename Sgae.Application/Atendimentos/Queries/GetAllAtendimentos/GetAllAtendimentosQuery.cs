using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Queries.GetAllAtendimentos;

/// <summary>
/// Consulta estruturada para obter os Atendimentos Espirituais registrados no sistema de forma paginada e eficiente.
/// </summary>
public record GetAllAtendimentosQuery : PaginationQuery, IQuery<PagedResponse<AtendimentoEspiritualDto>>;
