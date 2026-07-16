using Dapper;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Leads.DTOs;
using Sgae.Application.Perfis.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadById;

/// <summary>
/// Manipulador da consulta GetLeadByIdQuery otimizado com Dapper para realizar leituras de alta performance (CQRS).
/// </summary>
public class GetLeadByIdQueryHandler : IQueryHandler<GetLeadByIdQuery, LeadDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetLeadByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
    }

    public async Task<LeadDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        // Query SQL otimizada com JOIN para carregar o Lead e seu Perfil associado em uma única viagem ao banco de dados (single roundtrip)
        const string sql = @"
            SELECT 
                l.""Id"", l.""Nome"", l.""Telefone"", l.""Email"", l.""Cidade"", l.""Estado"", l.""Origem"", l.""ProblemaPrincipal"", l.""DataContato"" AS ""DataCaptacao"",
                p.""Id"", p.""Idade"", p.""FaixaEtaria"", p.""Genero"", p.""Profissao"", p.""Escolaridade"", p.""EstadoCivil"", p.""LeadId""
            FROM ""Leads"" l
            LEFT JOIN ""PerfisConsulentes"" p ON l.""Id"" = p.""LeadId"" AND p.""IsDeleted"" = false
            WHERE l.""Id"" = @Id AND l.""IsDeleted"" = false";

        var result = await connection.QueryAsync<LeadDto, PerfilConsulenteDto, LeadDto>(
            sql,
            (lead, perfil) =>
            {
                if (perfil != null && perfil.Id != Guid.Empty)
                {
                    lead.Perfil = perfil;
                }
                return lead;
            },
            new { Id = request.Id },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }
}
