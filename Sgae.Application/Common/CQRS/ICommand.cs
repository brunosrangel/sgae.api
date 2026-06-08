using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato base de CQRS para Commands que retornam um tipo de dado específico (ex: Guid do registro criado).
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Contrato base de CQRS para Commands sem retorno explícito (Void).
/// </summary>
public interface ICommand : IRequest
{
}