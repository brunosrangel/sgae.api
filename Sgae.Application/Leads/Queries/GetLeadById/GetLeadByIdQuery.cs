using Sgae.Application.Common.CQRS;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadById;

/// <summary>
/// Consulta CQRS para obter os detalhes de um determinado Consulente (Lead) pelo seu identificador primário.
/// </summary>
public record GetLeadByIdQuery(Guid Id) : IQuery<LeadDto?>;
