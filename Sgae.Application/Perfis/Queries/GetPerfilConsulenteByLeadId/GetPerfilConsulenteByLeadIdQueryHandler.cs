using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Perfis.DTOs;

namespace Sgae.Application.Perfis.Queries.GetPerfilConsulenteByLeadId;

/// <summary>
/// Manipulador que recupera o perfil de um consulente usando Dapper e suporte a cache distribuído.
/// </summary>
public class GetPerfilConsulenteByLeadIdQueryHandler : IQueryHandler<GetPerfilConsulenteByLeadIdQuery, PerfilConsulenteDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDistributedCache _cache;

    public GetPerfilConsulenteByLeadIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IDistributedCache cache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _cache = cache;
    }

    public async Task<PerfilConsulenteDto?> Handle(GetPerfilConsulenteByLeadIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"perfil_lead_{request.LeadId}";
        
        // 1. Tentar recuperar dados do Cache Distribuído
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedPerfil = JsonSerializer.Deserialize<PerfilConsulenteDto>(cachedData);
            if (cachedPerfil != null)
            {
                return cachedPerfil;
            }
        }

        // 2. Se falhar, consultar o banco via Dapper para alta performance
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                ""Id"", ""LeadId"", ""Idade"", ""FaixaEtaria"", ""Genero"", ""Profissao"", ""Escolaridade"", ""EstadoCivil""
            FROM ""PerfisConsulentes""
            WHERE ""LeadId"" = @LeadId AND ""IsDeleted"" = false";

        var result = await connection.QueryFirstOrDefaultAsync<PerfilConsulenteDto>(
            sql,
            new { LeadId = request.LeadId }
        );

        // 3. Serializar e guardar no Cache se encontrado
        if (result != null)
        {
            var serializedData = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(
                cacheKey,
                serializedData,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) },
                cancellationToken
            );
        }

        return result;
    }
}
