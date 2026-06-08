using System;

namespace Sgae.Domain.Exceptions;

/// <summary>
/// Classe de exceção base para regras de negócio e violações de invariantes de domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}