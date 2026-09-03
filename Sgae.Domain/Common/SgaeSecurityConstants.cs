namespace Sgae.Domain.Common;

/// <summary>
/// Definição centralizada de Roles (Perfis) de acesso no sistema SGAE.
/// </summary>
public static class SgaeRoles
{
    public const string Admin = "Admin";
    public const string Sacerdote = "Sacerdote";
    public const string Secretaria = "Secretaria";
    public const string Consulente = "Consulente";
}

/// <summary>
/// Definição de Políticas de Autorização (RBAC) corporativas no sistema SGAE.
/// </summary>
public static class SgaePolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string SacerdoteOnly = "SacerdoteOnly";
    public const string SecretariaOnly = "SecretariaOnly";
    public const string ConsulenteOnly = "ConsulenteOnly";
    public const string EquipePastoral = "EquipePastoral";
    public const string AtendimentoEspiritual = "AtendimentoEspiritual";
    public const string RecepcaoEAgendamento = "RecepcaoEAgendamento";
}
