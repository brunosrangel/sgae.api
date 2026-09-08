namespace Sgae.Application.Abstractions;

/// <summary>
/// Contrato do serviço responsável por semear e garantir a integridade dos usuários administrativos,
/// papéis de autorização (Roles) e credenciais essenciais do sistema SGAE durante a inicialização.
/// </summary>
public interface IIdentitySeeder
{
    /// <summary>
    /// Executa de forma idempotente a validação e criação/atualização dos perfis de acesso (Roles)
    /// e usuários administradores e sacerdotais padrão.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task SeedIdentityAsync(CancellationToken cancellationToken = default);
}
