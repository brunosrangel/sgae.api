using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadById;

/// <summary>
/// Manipulador da consulta GetLeadByIdQuery para retornar todos os dados ricos do Lead, Perfil e Histórico.
/// </summary>
public class GetLeadByIdQueryHandler : IQueryHandler<GetLeadByIdQuery, LeadDto?>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetLeadByIdQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<LeadDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .AsNoTracking()
            .Include(l => l.Perfil)
            .Include(l => l.Historico)
            .Include(l => l.CanalCaptacao)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lead == null)
            return null;

        return _mapper.Map<LeadDto>(lead);
    }
}
