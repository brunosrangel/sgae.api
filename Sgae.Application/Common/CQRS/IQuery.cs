using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato de CQRS para consultas (Query) que obrigatoriamente retornam um dado do tipo TResponse.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}