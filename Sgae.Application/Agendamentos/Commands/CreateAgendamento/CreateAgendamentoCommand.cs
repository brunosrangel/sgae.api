using System;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Comando contendo as especificações do agendamento da consulta espiritual.
/// </summary>
public record CreateAgendamentoCommand(
    Guid LeadId,
    DateTime DataHora,
    ModalidadeAtendimento Modalidade,
    decimal Valor
) : ICommand<Guid>;