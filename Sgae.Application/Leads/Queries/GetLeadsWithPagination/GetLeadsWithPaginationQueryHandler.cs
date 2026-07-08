using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadsWithPagination;

public class GetLeadsWithPaginationQueryHandler : IQueryHandler<GetLeadsWithPaginationQuery, PagedResult<LeadDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetLeadsWithPaginationQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<LeadDto>> Handle(GetLeadsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leads.AsNoTracking();

        // 1. Aplica filtros dinâmicos
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(l => l.Nome.ToLower().Contains(search) || 
                                     l.Email.ToLower().Contains(search) || 
                                     l.Cidade.ToLower().Contains(search));
        }

        if (request.DataInicio.HasValue)
        {
            query = query.Where(l => l.DataContato >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            query = query.Where(l => l.DataContato <= request.DataFim.Value);
        }

        // 2. Ordenação padrão por data de captação decrescente
        query = query.OrderByDescending(l => l.DataContato);

        // 3. Projeta diretamente em DTO usando AutoMapper para otimização de Select (QueryableExtensions) e pagina usando o helper
        return await query.ProjectTo<LeadDto>(_mapper.ConfigurationProvider)
            .ToPagedResultAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );
    }
}