using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Atendimentos.DTOs;

namespace Sgae.Application.Atendimentos.Queries.GetAllAtendimentos;

/// <summary>
/// Manipulador de consulta que recupera os Atendimentos Espirituais de forma paginada usando Dapper e suporte a cache distribuído baseado nos parâmetros da página.
/// </summary>
public class GetAllAtendimentosQueryHandler : IQueryHandler<GetAllAtendimentosQuery, PagedResponse<AtendimentoEspiritualDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDistributedCache _cache;

    public GetAllAtendimentosQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IDistributedCache cache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _cache = cache;
    }

    public async Task<PagedResponse<AtendimentoEspiritualDto>> Handle(GetAllAtendimentosQuery request, CancellationToken cancellationToken)
    {
        // Safe guards para garantir integridade matemática da paginação
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var offset = (pageNumber - 1) * pageSize;

        var cacheKey = $"atendimentos_page_{pageNumber}_size_{pageSize}";

        // 1. Tentar ler do Cache Distribuído
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedResult = JsonSerializer.Deserialize<PagedResponse<AtendimentoEspiritualDto>>(cachedData);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        // 2. Consultar o total de registros para cálculo correto da paginação
        const string countSql = @"
            SELECT COUNT(*) 
            FROM ""AtendimentosEspirituais"" 
            WHERE ""IsDeleted"" = false";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        if (totalCount == 0)
        {
            var emptyResponse = new PagedResponse<AtendimentoEspiritualDto>(new List<AtendimentoEspiritualDto>(), 0, pageNumber, pageSize);
            return emptyResponse;
        }

        // 3. Consultar a página específica usando Limit e Offset para otimização do banco
        const string paginatedSql = @"
            SELECT 
                ""Id"", 
                ""AgendamentoId"", 
                ""Tipo"", 
                ""TempoDuracaoMinutos"", 
                ""TemasAbordados"", 
                ""Observacoes"", 
                ""CreatedAt"" 
            FROM ""AtendimentosEspirituais"" 
            WHERE ""IsDeleted"" = false 
            ORDER BY ""CreatedAt"" DESC
            LIMIT @Limit OFFSET @Offset";

        var result = await connection.QueryAsync<AtendimentoEspiritualDto>(
            paginatedSql,
            new { Limit = pageSize, Offset = offset }
        );

        var list = result.ToList();
        foreach (var item in list)
        {
            item.TipoDescricao = item.Tipo.ToString();
        }

        var pagedResponse = new PagedResponse<AtendimentoEspiritualDto>(list, totalCount, pageNumber, pageSize);

        // 4. Armazenar em cache o resultado paginado por 10 minutos
        var serializedData = JsonSerializer.Serialize(pagedResponse);
        await _cache.SetStringAsync(
            cacheKey, 
            serializedData, 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }, 
            cancellationToken
        );

        return pagedResponse;
    }
}
