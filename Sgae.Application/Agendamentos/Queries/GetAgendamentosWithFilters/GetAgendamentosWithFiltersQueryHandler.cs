using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Agendamentos.DTOs;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentosWithFilters;

public class GetAgendamentosWithFiltersQueryHandler : IQueryHandler<GetAgendamentosWithFiltersQuery, List<AgendamentoDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAgendamentosWithFiltersQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AgendamentoDto>> Handle(GetAgendamentosWithFiltersQuery request, CancellationToken cancellationToken)
    {
        // Carrega as entidades de forma otimizada com Includes e projeta diretamente
        var query = _context.Agendamentos
            .Include(a => a.Lead) // Garante o carregamento dos dados do Consulente para mapeamento legal
            .Include(a => a.Atendimento) // Inclui o atendimento espiritual para Etapa 4
            .AsNoTracking();

        // Aplicando os filtros inteligentes
        if (request.DataInicio.HasValue)
        {
            query = query.Where(a => a.DataHora >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            query = query.Where(a => a.DataHora <= request.DataFim.Value);
        }

        if (request.Modalidade.HasValue)
        {
            query = query.Where(a => a.Modalidade == request.Modalidade.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // Ordenado cronologicamente
        query = query.OrderBy(a => a.DataHora);

        // Retorna a projeção limpa via AutoMapper
        return await query
            .ProjectTo<AgendamentoDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}