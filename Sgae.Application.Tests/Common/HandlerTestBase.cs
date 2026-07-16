using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Sgae.Application.Abstractions;

namespace Sgae.Application.Tests.Common;

/// <summary>
/// Classe base abstrata que provê toda infraestrutura e abstração necessárias para testar
/// unitariamente os manipuladores MediatR (Command e Query Handlers) usando Moq e FluentAssertions.
/// </summary>
public abstract class HandlerTestBase
{
    protected Mock<IUnitOfWork> MockUnitOfWork { get; }
    protected Mock<IDistributedCache> MockCache { get; }

    protected HandlerTestBase()
    {
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockCache = new Mock<IDistributedCache>();

        // Configura o UnitOfWork para retornar sucesso (1) por padrão no commit transacional
        MockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    /// <summary>
    /// Facilita a criação dinâmica de mocks para repositórios ou dependências adicionais.
    /// </summary>
    /// <typeparam name="T">O tipo da interface/classe a ser mockada.</typeparam>
    /// <returns>Uma nova instância mockada.</returns>
    protected Mock<T> CreateMock<T>() where T : class
    {
        return new Mock<T>();
    }

    /// <summary>
    /// Fornece um mock funcional e isolado de ILogger do Microsoft.Extensions.Logging.
    /// </summary>
    /// <typeparam name="T">O tipo associado ao Logger.</typeparam>
    /// <returns>Objeto ILogger pronto para uso.</returns>
    protected ILogger<T> CreateLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }
}
