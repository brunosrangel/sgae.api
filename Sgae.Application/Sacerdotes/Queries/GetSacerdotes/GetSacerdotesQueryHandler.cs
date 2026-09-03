using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Sacerdotes.DTOs;

namespace Sgae.Application.Sacerdotes.Queries.GetSacerdotes;

public class GetSacerdotesQueryHandler : IQueryHandler<GetSacerdotesQuery, List<SacerdoteDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetSacerdotesQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SacerdoteDto>> Handle(GetSacerdotesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sacerdotes.AsNoTracking();

        if (request.ApenasAtivos.HasValue && request.ApenasAtivos.Value)
        {
            query = query.Where(s => s.Ativo);
        }

        var sacerdotes = await query
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<SacerdoteDto>>(sacerdotes);
    }
}
