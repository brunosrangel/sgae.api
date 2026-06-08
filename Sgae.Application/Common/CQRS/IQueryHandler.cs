using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato para manipuladores de consultas CQRS.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}