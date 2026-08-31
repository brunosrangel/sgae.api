using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentoById;

public class GetAgendamentoByIdQueryHandler : IQueryHandler<GetAgendamentoByIdQuery, AgendamentoDto?>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAgendamentoByIdQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AgendamentoDto?> Handle(GetAgendamentoByIdQuery request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.Lead)
            .Include(a => a.Sacerdote)
            .Include(a => a.ServicoConsulta)
            .Include(a => a.Atendimento)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (agendamento == null)
            return null;

        return _mapper.Map<AgendamentoDto>(agendamento);
    }
}
