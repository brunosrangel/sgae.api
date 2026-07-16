using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Sgae.Application.Abstractions;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Common.CQRS;
using System.Data;
using System.Text.Json;

namespace Sgae.Application.Atendimentos.Queries.GetAcompanhamentosByAtendimentoId;

/// <summary>
/// Manipulador de consulta para obter acompanhamentos via Dapper com suporte a cache distribuído.
/// </summary>
public class GetAcompanhamentosByAtendimentoIdQueryHandler : IQueryHandler<GetAcompanhamentosByAtendimentoIdQuery, List<AcompanhamentoDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDistributedCache _cache;

    public GetAcompanhamentosByAtendimentoIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IDistributedCache cache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _cache = cache;
    }

    public async Task<List<AcompanhamentoDto>> Handle(GetAcompanhamentosByAtendimentoIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"acompanhamentos_atendimento_{request.AtendimentoEspiritualId}";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedList = JsonSerializer.Deserialize<List<AcompanhamentoDto>>(cachedData);
            if (cachedList != null)
            {
                return cachedList;
            }
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                ""Id"", 
                ""AtendimentoEspiritualId"", 
                ""DataAcompanhamento"", 
                ""SintomasMelhora"", 
                ""Recomendacoes"", 
                ""Observacoes"", 
                ""CreatedAt"" 
            FROM ""Acompanhamentos"" 
            WHERE ""AtendimentoEspiritualId"" = @AtendimentoEspiritualId AND ""IsDeleted"" = false 
            ORDER BY ""DataAcompanhamento"" DESC";

        var result = await connection.QueryAsync<AcompanhamentoDto>(
            sql,
            new { AtendimentoEspiritualId = request.AtendimentoEspiritualId }
        );

        var list = result.ToList();

        var serializedData = JsonSerializer.Serialize(list);
        await _cache.SetStringAsync(
            cacheKey,
            serializedData,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(10) },
            cancellationToken
        );

        return list;
    }
}
