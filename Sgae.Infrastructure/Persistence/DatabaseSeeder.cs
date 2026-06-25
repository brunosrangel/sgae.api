using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
