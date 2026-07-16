using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadsWithPagination;

public record GetLeadsWithPaginationQuery : IQuery<PagedResult<LeadDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
}