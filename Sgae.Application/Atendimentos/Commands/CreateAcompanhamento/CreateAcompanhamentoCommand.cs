using System;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;

/// <summary>
/// Comando para registrar um novo Acompanhamento (Monitoramento de evolução).
/// </summary>
public record CreateAcompanhamentoCommand(
    Guid AtendimentoEspiritualId,
    DateTime DataAcompanhamento,
    string SintomasMelhora,
    string Recomendacoes,
    string Observacoes
) : ICommand<Guid>;
