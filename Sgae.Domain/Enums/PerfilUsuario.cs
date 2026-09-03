namespace Sgae.Domain.Enums;

/// <summary>
/// Perfis de acesso e privilégios de usuário no sistema SGAE.
/// </summary>
public enum PerfilUsuario
{
    /// <summary>
    /// Administrador do sistema com privilégios irrestritos.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Sacerdote responsável pelas consultas, leituras de oráculos, rituais e prescrições espirituais.
    /// </summary>
    Sacerdote = 2,

    /// <summary>
    /// Equipe de secretaria / recepção responsável pelo acolhimento de leads, agendamentos e suporte.
    /// </summary>
    Secretaria = 3,

    /// <summary>
    /// Consulente / visitante com acesso ao próprio histórico e agendamentos.
    /// </summary>
    Consulente = 4
}
