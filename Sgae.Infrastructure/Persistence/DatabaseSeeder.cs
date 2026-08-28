using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Provedor robusto e seguro para execução automática de migrações e seeding estruturado de dados pastorais iniciais e de configurações do sistema.
/// </summary>
public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inicializa a infraestrutura de dados (migrations) e popula os cadastros de referência se estiverem vazios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task InitializeAndSeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SGAE Seeder: Iniciando validação de infraestrutura e migração estruturada do banco de dados...");

        try
        {
            // Executa a estratégia de coexistência entre migrations EF Core e EnsureCreated de forma tolerante a falhas
            var hasPendingMigrations = false;
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
                hasPendingMigrations = pendingMigrations.Any();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SGAE Seeder: Não foi possível obter migrações pendentes - o projeto pode não conter migrations compiladas ainda.");
            }

            if (hasPendingMigrations)
            {
                _logger.LogInformation("SGAE Seeder: Aplicando migrações EF Core pendentes de forma segura no PostgreSQL...");
                await _context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("SGAE Seeder: Nenhuma migração pendente encontrada. Garantindo criação física do banco...");
                await _context.Database.EnsureCreatedAsync(cancellationToken);
            }

            // Garante de forma idempotente a existência de todas as novas colunas e tabelas no banco relacional PostgreSQL
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""CustomId"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Nome"" VARCHAR(150) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Telefone"" VARCHAR(20) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Email"" VARCHAR(150) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""DataNascimento"" TIMESTAMP WITH TIME ZONE NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Profissao"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Nacionalidade"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Naturalidade"" VARCHAR(150) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""TradicaoTerreiro"" VARCHAR(500) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""VinculoTradicoes"" VARCHAR(200) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""VinculoCcrias"" VARCHAR(50) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Temporalidade"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""JogouBuziosBabalorisaSidnei"" VARCHAR(50) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""OrixasNagoKetu"" TEXT NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Cep"" VARCHAR(20) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Endereco"" VARCHAR(250) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Numero"" VARCHAR(50) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Complemento"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Bairro"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Cidade"" VARCHAR(100) NOT NULL DEFAULT 'São Paulo';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Estado"" VARCHAR(2) NOT NULL DEFAULT 'SP';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""DataContato"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Origem"" VARCHAR(50) NOT NULL DEFAULT 'Organico';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""ProblemaPrincipal"" VARCHAR(2000) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Observacoes"" VARCHAR(2000) NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Status"" VARCHAR(50) NOT NULL DEFAULT 'Novo';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""Prioridade"" VARCHAR(50) NOT NULL DEFAULT 'Média';
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""CanalCaptacaoId"" UUID NULL;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""Leads"" ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL;

                    UPDATE ""Leads"" SET ""OrixasNagoKetu"" = '[]' WHERE ""OrixasNagoKetu"" IS NULL;

                    CREATE TABLE IF NOT EXISTS ""LeadsHistoricos"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""LeadId"" UUID NOT NULL REFERENCES ""Leads""(""Id"") ON DELETE CASCADE,
                        ""Data"" TIMESTAMP WITH TIME ZONE NOT NULL,
                        ""Tipo"" VARCHAR(100) NOT NULL,
                        ""Descricao"" VARCHAR(1000) NOT NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE TABLE IF NOT EXISTS ""Sacerdotes"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nome"" VARCHAR(150) NOT NULL,
                        ""Cargo"" VARCHAR(100) NULL,
                        ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE TABLE IF NOT EXISTS ""ServicosConsulta"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nome"" VARCHAR(150) NOT NULL,
                        ""Descricao"" VARCHAR(500) NULL,
                        ""ValorBase"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE TABLE IF NOT EXISTS ""Agendamentos"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""LeadId"" UUID NOT NULL REFERENCES ""Leads""(""Id"") ON DELETE CASCADE,
                        ""DataHora"" TIMESTAMP WITH TIME ZONE NOT NULL,
                        ""Modalidade"" VARCHAR(50) NOT NULL DEFAULT 'Presencial',
                        ""Valor"" DECIMAL(18, 2) NOT NULL DEFAULT 0,
                        ""Status"" VARCHAR(50) NOT NULL DEFAULT 'Pendente',
                        ""MotivoCancelamento"" VARCHAR(500) NULL,
                        ""SacerdoteId"" UUID NULL REFERENCES ""Sacerdotes""(""Id"") ON DELETE SET NULL,
                        ""ServicoConsultaId"" UUID NULL REFERENCES ""ServicosConsulta""(""Id"") ON DELETE SET NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""SacerdoteId"" UUID NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""ServicoConsultaId"" UUID NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""MotivoCancelamento"" VARCHAR(500) NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""Modalidade"" VARCHAR(50) NOT NULL DEFAULT 'Presencial';
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""Status"" VARCHAR(50) NOT NULL DEFAULT 'Pendente';
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""Valor"" DECIMAL(18, 2) NOT NULL DEFAULT 0;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""DataHora"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL;

                    CREATE TABLE IF NOT EXISTS ""CanaisCaptacao"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nome"" VARCHAR(150) NOT NULL UNIQUE,
                        ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );
                ", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SGAE Seeder: Aviso ao sincronizar esquema adicional de tabelas.");
            }

            _logger.LogInformation("SGAE Seeder: Infraestrutura de tabelas assegurada de forma íntegra.");

            // Executa o Seeding dos Cargos Pastorais (SGAE Pastoral Roles)
            await SeedPastoralRolesAsync(cancellationToken);

            // Executa o Seeding das Configurações do Sistema (System Configurations)
            await SeedSystemConfigurationsAsync(cancellationToken);

            // Executa o Seeding das Categorias de Atendimento Espiritual (Spiritual Attendance Categories)
            await SeedSpiritualAttendanceCategoriesAsync(cancellationToken);

            _logger.LogInformation("SGAE Seeder: Carga inicial de dados finalizada com pleno sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "SGAE Seeder: Erro catastrófico ao inicializar e semear o banco de dados.");
            throw;
        }
    }

    private async Task SeedPastoralRolesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SGAE Seeder: Verificando tabela de papeis/cargos pastorais...");

        var hasRoles = await _context.PastoralRoles.AnyAsync(cancellationToken);
        if (!hasRoles)
        {
            _logger.LogInformation("SGAE Seeder: Tabela de cargos pastorais vazia. Semeando perfis referenciais...");

            var roles = new[]
            {
                new PastoralRole(
                    "Admin",
                    "Administrador de sistemas com privilégios irrestritos de configuração, auditoria e controle de acessos.",
                    "System.All,Pastoral.All"
                ),
                new PastoralRole(
                    "Pastor",
                    "Membro da equipe pastoral responsável por triagens, aconselhamento fraterno, passes e acompanhamento dos consulentes.",
                    "Pastoral.Atendimento,Pastoral.Acompanhamento,Pastoral.Leitura"
                ),
                new PastoralRole(
                    "Coordenador",
                    "Coordenador do fluxo de acolhimento encarregado do controle de leads de captação, agendamentos e status de salas.",
                    "Pastoral.Agendamento,Pastoral.Lead,Pastoral.Leitura"
                ),
                new PastoralRole(
                    "PastoralStaff",
                    "Auxiliar de apoio pastoral focado na acolhida inicial, suporte logístico e leitura de relatórios de presença.",
                    "Pastoral.Leitura"
                )
            };

            await _context.PastoralRoles.AddRangeAsync(roles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE Seeder: Semeados 4 cargos pastorais com sucesso no banco de dados.");
        }
        else
        {
            _logger.LogInformation("SGAE Seeder: Dados de cargos pastorais já presentes. Pulo executado de forma segura.");
        }
    }

    private async Task SeedSystemConfigurationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SGAE Seeder: Verificando tabela de configurações gerais de negócio...");

        var hasConfigs = await _context.SystemConfigurations.AnyAsync(cancellationToken);
        if (!hasConfigs)
        {
            _logger.LogInformation("SGAE Seeder: Tabela de configurações vazia. Semeando parâmetros operacionais...");

            var configs = new[]
            {
                new SystemConfiguration(
                    "JwtExpirationMinutes",
                    "120",
                    "Define o tempo máximo de expiração do Token JWT (em minutos) antes de requerer reautenticação."
                ),
                new SystemConfiguration(
                    "RateLimitPermitLimit",
                    "30",
                    "Limite de requisições por minuto toleradas para endpoints sensíveis de atendimento pastoral."
                ),
                new SystemConfiguration(
                    "DatabaseCacheMinutes",
                    "60",
                    "Tempo padrão de sobrevivência das listagens pastorais no cache distribuído estruturado."
                ),
                new SystemConfiguration(
                    "MinPastoralSessionDuration",
                    "15",
                    "Duração mínima sugerida em minutos para um atendimento visando qualidade pastoral."
                ),
                new SystemConfiguration(
                    "ThemeLayoutValue",
                    "CorporateSlate",
                    "Configuração estética do dashboard do consolador (CorporateSlate, AmberSerene, MidnightSpace, PastelAura)."
                )
            };

            await _context.SystemConfigurations.AddRangeAsync(configs, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE Seeder: Semeados 5 parâmetros de configuração geral com sucesso.");
        }
        else
        {
            _logger.LogInformation("SGAE Seeder: Configurações do sistema já presentes. Pulo executado de forma segura.");
        }
    }

    private async Task SeedSpiritualAttendanceCategoriesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SGAE Seeder: Verificando tabela de categorias de atendimento espiritual...");

        var hasCategories = await _context.SpiritualAttendanceCategories.AnyAsync(cancellationToken);
        if (!hasCategories)
        {
            _logger.LogInformation("SGAE Seeder: Tabela de categorias vazia. Semeando tipos de atendimentos pastorais...");

            var categories = new[]
            {
                new SpiritualAttendanceCategory(
                    TipoAtendimento.TratamentoEspiritual,
                    "Tratamento Espiritual",
                    "Tratamento focado na harmonização energética e reequilíbrio espiritual por meio de passes específicos e fluidoterapia direcionada.",
                    40
                ),
                new SpiritualAttendanceCategory(
                    TipoAtendimento.Desobsessao,
                    "Desobsessão",
                    "Sessão de esclarecimento e desobsessão para auxílio a entidades necessitadas e reabilitação espiritual profunda do consulente.",
                    50
                ),
                new SpiritualAttendanceCategory(
                    TipoAtendimento.AssistenciaFraterna,
                    "Assistência Fraterna",
                    "Atendimento acolhedor baseado em conversação fraterna, escuta ativa e direcionamento evangélico-doutrinário inicial.",
                    30
                ),
                new SpiritualAttendanceCategory(
                    TipoAtendimento.PasseEspiritual,
                    "Passe Espiritual",
                    "Transmissão purificadora de fluidos e bioenergia salutares para reestabelecimento psicossomático seguro.",
                    15
                ),
                new SpiritualAttendanceCategory(
                    TipoAtendimento.Doutrinacao,
                    "Doutrinação",
                    "Reunião dedicada ao esclarecimento doutrinário de cariz terapêutico para espíritos desencarnados aflitos.",
                    45
                ),
                new SpiritualAttendanceCategory(
                    TipoAtendimento.Outros,
                    "Outros Atendimentos",
                    "Ações diversas de acolhida pastoral e suporte espiritual personalizado não descritas nas categorias eminentes.",
                    20
                )
            };

            await _context.SpiritualAttendanceCategories.AddRangeAsync(categories, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE Seeder: Semeadas 6 categorias de atendimento espiritual de referência.");
        }
        else
        {
            _logger.LogInformation("SGAE Seeder: Dados de categorias de atendimento espiritual já presentes. Pulo executado de forma segura.");
        }
    }
}
