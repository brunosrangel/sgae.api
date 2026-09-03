namespace Sgae.Application.Abstractions;

/// <summary>
/// Contrato para geração segura e verificação de hashes de senha (PBKDF2/SHA-512 com salt criptográfico).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gera um hash criptográfico seguro a partir de uma senha em texto plano.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifica se uma senha fornecida corresponde ao hash armazenado.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
