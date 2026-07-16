using Microsoft.Extensions.Configuration;
using Npgsql;
using Sgae.Application.Abstractions;
using System.Data;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Implementação física da fábrica de conexões SQL baseada em PostgreSQL (Npgsql) para uso com o Dapper.
/// </summary>
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentException("A connection string 'DefaultConnection' não foi configurada.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
