using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato para manipuladores de Comandos que possuem resposta do tipo TResponse.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Contrato para manipuladores de Comandos que não retornam dados.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}