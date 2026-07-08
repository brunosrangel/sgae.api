using System;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Leads.Commands.DeleteLead;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para remover/excluir um Consulente (Lead).
/// </summary>
public record DeleteLeadCommand(Guid Id) : ICommand;
