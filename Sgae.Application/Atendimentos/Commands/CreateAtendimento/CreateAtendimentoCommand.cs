using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Atendimentos.Commands.CreateAtendimento;

/// <summary>
/// Comando para registrar um novo Atendimento Espiritual (Etapa 4).
/// </summary>
public record CreateAtendimentoCommand(
    Guid AgendamentoId,
    TipoAtendimento Tipo,
    int TempoDuracaoMinutos,
    string TemasAbordados,
    string Observacoes
) : ICommand<Guid>;
