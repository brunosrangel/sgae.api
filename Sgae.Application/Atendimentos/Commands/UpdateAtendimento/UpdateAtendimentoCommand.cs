using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Atendimentos.Commands.UpdateAtendimento;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para atualizar um Atendimento Espiritual.
/// </summary>
public record UpdateAtendimentoCommand(
    Guid Id,
    TipoAtendimento Tipo,
    int TempoDuracaoMinutos,
    string TemasAbordados,
    string Observacoes
) : ICommand;
