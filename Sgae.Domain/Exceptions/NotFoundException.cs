namespace Sgae.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma entidade solicitada não é encontrada no repositório/banco de dados.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entidade \"{name}\" ({key}) não foi encontrada.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
