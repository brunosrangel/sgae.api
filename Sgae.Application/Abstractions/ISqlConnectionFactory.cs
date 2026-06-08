using System.Data;

namespace Sgae.Application.Abstractions;

/// <summary>
/// Fábrica de conexões SQL dedicadas para otimizar consultas complexas do lado de leitura (CQRS) via Dapper.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Cria e retorna uma nova conexão com o banco de dados.
    /// </summary>
    IDbConnection CreateConnection();
}
