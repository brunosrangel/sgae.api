using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Leads.Commands.UpdateLead;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para atualizar um Consulente (Lead).
/// </summary>
public record UpdateLeadCommand(
    Guid Id,
    string Nome,
    string Telefone,
    string Email,
    string Cidade,
    string Estado,
    string ProblemaPrincipal
) : ICommand;
