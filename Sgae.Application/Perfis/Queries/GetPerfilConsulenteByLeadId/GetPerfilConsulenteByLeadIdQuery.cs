using Sgae.Application.Common.CQRS;
using Sgae.Application.Perfis.DTOs;

namespace Sgae.Application.Perfis.Queries.GetPerfilConsulenteByLeadId;

/// <summary>
/// Consulta CQRS para obter as informações de perfil demográfico associadas a um Consulente (Lead).
/// </summary>
public record GetPerfilConsulenteByLeadIdQuery(Guid LeadId) : IQuery<PerfilConsulenteDto?>;
