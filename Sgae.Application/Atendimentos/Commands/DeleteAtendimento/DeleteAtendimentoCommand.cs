using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Atendimentos.Commands.DeleteAtendimento;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para remover/excluir um Atendimento Espiritual.
/// </summary>
public record DeleteAtendimentoCommand(Guid Id) : ICommand;
