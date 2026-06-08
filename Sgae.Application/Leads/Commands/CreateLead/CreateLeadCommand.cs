using System;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para a captação de um novo Consulente (Lead).
/// </summary>
public record CreateLeadCommand(
    string Nome,
    string Telefone,
    string Email,
    string Cidade,
    string Estado,
    OrigemContato Origem,
    string ProblemaPrincipal
) : ICommand<Guid>;