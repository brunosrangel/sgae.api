using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Queries.GetAtendimentoById;

/// <summary>
/// Manipulador de consulta que recupera um Atendimento Espiritual usando Dapper com suporte a cache distribuído.
/// </summary>
public class GetAtendimentoByIdQueryHandler : IQueryHandler<GetAtendimentoByIdQuery, AtendimentoEspiritualDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDistributedCache _cache;

    public GetAtendimentoByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IDistributedCache cache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _cache = cache;
    }

    public async Task<AtendimentoEspiritualDto?> Handle(GetAtendimentoByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"atendimento_{request.Id}";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<AtendimentoEspiritualDto>(cachedData);
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                ""Id"", 
                ""AgendamentoId"", 
                ""Tipo"", 
                ""TempoDuracaoMinutos"", 
                ""TemasAbordados"", 
                ""Observacoes"", 
                ""CreatedAt"" 
            FROM ""AtendimentosEspirituais"" 
            WHERE ""Id"" = @Id AND ""IsDeleted"" = false";

        var result = await connection.QueryFirstOrDefaultAsync<AtendimentoEspiritualDto>(
            sql, 
            new { Id = request.Id }
        );

        if (result != null)
        {
            result.TipoDescricao = result.Tipo.ToString();

            var serializedData = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(
                cacheKey, 
                serializedData, 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(10) }, 
                cancellationToken
            );
        }

        return result;
    }
}
