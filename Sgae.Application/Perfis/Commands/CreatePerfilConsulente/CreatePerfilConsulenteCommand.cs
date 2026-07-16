using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Perfis.Commands.CreatePerfilConsulente;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para estabelecer o Perfil do Consulente.
/// </summary>
public record CreatePerfilConsulenteCommand(
    Guid LeadId,
    int Idade,
    string FaixaEtaria,
    string Genero,
    string Profissao,
    string Escolaridade,
    string EstadoCivil
) : ICommand<Guid>;
