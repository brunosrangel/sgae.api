using Sgae.Application.Common.CQRS;
using Sgae.Application.Sacerdotes.DTOs;

namespace Sgae.Application.Sacerdotes.Queries.GetSacerdotes;

public class GetSacerdotesQuery : IQuery<List<SacerdoteDto>>
{
    public bool? ApenasAtivos { get; set; } = true;
}
