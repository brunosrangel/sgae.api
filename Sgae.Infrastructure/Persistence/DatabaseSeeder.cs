using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Provedor robusto e seguro para execução automática de migrações e seeding estruturado de dados pastorais iniciais, usuários e de configurações do sistema.
/// </summary>
public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger, IPasswordHasher passwordHasher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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
                        ""Especialidade"" VARCHAR(500) NULL,
                        ""Cargo"" VARCHAR(100) NULL,
                        ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    ALTER TABLE IF EXISTS ""Sacerdotes"" ADD COLUMN IF NOT EXISTS ""Especialidade"" VARCHAR(500) NULL;
                    ALTER TABLE IF EXISTS ""Sacerdotes"" ADD COLUMN IF NOT EXISTS ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE;
                    ALTER TABLE IF EXISTS ""Sacerdotes"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;

                    CREATE TABLE IF NOT EXISTS ""ServicosConsulta"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nome"" VARCHAR(150) NOT NULL,
                        ""Tarifa"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ""Descricao"" VARCHAR(500) NULL,
                        ""ValorBase"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    ALTER TABLE IF EXISTS ""ServicosConsulta"" ADD COLUMN IF NOT EXISTS ""Tarifa"" DECIMAL(18,2) NOT NULL DEFAULT 0;
                    ALTER TABLE IF EXISTS ""ServicosConsulta"" ADD COLUMN IF NOT EXISTS ""Ativo"" BOOLEAN NOT NULL DEFAULT TRUE;
                    ALTER TABLE IF EXISTS ""ServicosConsulta"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;

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
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""FormaPagamento"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""Pago"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""Observacoes"" VARCHAR(1000) NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""WhatsappConfirmacaoDisparada"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""ConfigLembrete"" VARCHAR(100) NULL;
                    ALTER TABLE IF EXISTS ""Agendamentos"" ADD COLUMN IF NOT EXISTS ""FrequenciaLembrete"" VARCHAR(100) NULL;
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

                    CREATE TABLE IF NOT EXISTS ""Usuarios"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nome"" VARCHAR(150) NOT NULL,
                        ""Email"" VARCHAR(150) NOT NULL,
                        ""PasswordHash"" VARCHAR(500) NOT NULL,
                        ""Perfil"" VARCHAR(50) NOT NULL,
                        ""StatusAtivo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""PrimeiroAcesso"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""UltimoAcesso"" TIMESTAMP WITH TIME ZONE NULL,
                        ""SacerdoteId"" UUID NULL REFERENCES ""Sacerdotes""(""Id"") ON DELETE SET NULL,
                        ""PastoralRoleId"" UUID NULL REFERENCES ""PastoralRoles""(""Id"") ON DELETE SET NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_Email"" ON ""Usuarios"" (""Email"");

                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""Nome"" VARCHAR(150) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""Email"" VARCHAR(150) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""PasswordHash"" VARCHAR(500) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""Perfil"" VARCHAR(50) NOT NULL DEFAULT 'Admin';
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""StatusAtivo"" BOOLEAN NOT NULL DEFAULT TRUE;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""PrimeiroAcesso"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""UltimoAcesso"" TIMESTAMP WITH TIME ZONE NULL;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""SacerdoteId"" UUID NULL;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""PastoralRoleId"" UUID NULL;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""Usuarios"" ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL;

                    CREATE TABLE IF NOT EXISTS ""RefreshTokens"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Token"" VARCHAR(250) NOT NULL,
                        ""UsuarioId"" UUID NOT NULL REFERENCES ""Usuarios""(""Id"") ON DELETE CASCADE,
                        ""ExpiresAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                        ""IsRevoked"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""RevokedAt"" TIMESTAMP WITH TIME ZONE NULL,
                        ""CreatedByIp"" VARCHAR(50) NULL,
                        ""RevokedByIp"" VARCHAR(50) NULL,
                        ""ReplacedByToken"" VARCHAR(250) NULL,
                        ""ReasonRevoked"" VARCHAR(500) NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE TABLE IF NOT EXISTS ""Atendimentos"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""SacerdoteId"" UUID NOT NULL REFERENCES ""Sacerdotes""(""Id"") ON DELETE RESTRICT,
                        ""ConsulenteId"" UUID NOT NULL REFERENCES ""Leads""(""Id"") ON DELETE RESTRICT,
                        ""AgendamentoId"" UUID NULL REFERENCES ""Agendamentos""(""Id"") ON DELETE SET NULL,
                        ""DataConsulta"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""TipoOraculo"" VARCHAR(100) NOT NULL DEFAULT 'Jogo de Búzios',
                        ""PerguntaCentral"" VARCHAR(2000) NOT NULL DEFAULT '',
                        ""VeredictoEspiritual"" VARCHAR(4000) NOT NULL DEFAULT '',
                        ""Status"" VARCHAR(50) NOT NULL DEFAULT 'Realizado',
                        ""Observacoes"" VARCHAR(4000) NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE TABLE IF NOT EXISTS ""AnexosAtendimento"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""AtendimentoId"" UUID NOT NULL REFERENCES ""Atendimentos""(""Id"") ON DELETE CASCADE,
                        ""NomeArquivo"" VARCHAR(255) NOT NULL,
                        ""TipoArquivo"" VARCHAR(100) NOT NULL DEFAULT 'image/jpeg',
                        ""TamanhoBytes"" BIGINT NOT NULL DEFAULT 0,
                        ""Base64Data"" TEXT NULL,
                        ""Legenda"" VARCHAR(1000) NULL,
                        ""RotacaoGraus"" INT NOT NULL DEFAULT 0,
                        ""Categoria"" VARCHAR(50) NULL DEFAULT 'FotoBuzios',
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_SacerdoteId"" ON ""Atendimentos"" (""SacerdoteId"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_ConsulenteId"" ON ""Atendimentos"" (""ConsulenteId"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_DataConsulta"" ON ""Atendimentos"" (""DataConsulta"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_AgendamentoId"" ON ""Atendimentos"" (""AgendamentoId"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_Status"" ON ""Atendimentos"" (""Status"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_DataConsulta_Status"" ON ""Atendimentos"" (""DataConsulta"", ""Status"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_SacerdoteId_DataConsulta"" ON ""Atendimentos"" (""SacerdoteId"", ""DataConsulta"");
                    CREATE INDEX IF NOT EXISTS ""IX_Atendimentos_ConsulenteId_DataConsulta"" ON ""Atendimentos"" (""ConsulenteId"", ""DataConsulta"");
                    CREATE INDEX IF NOT EXISTS ""IX_AnexosAtendimento_AtendimentoId"" ON ""AnexosAtendimento"" (""AtendimentoId"");

                    CREATE TABLE IF NOT EXISTS ""AuditLogs"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""EntityName"" VARCHAR(150) NOT NULL,
                        ""EntityId"" VARCHAR(150) NOT NULL,
                        ""Action"" VARCHAR(50) NOT NULL,
                        ""UserIdentity"" VARCHAR(255) NOT NULL,
                        ""Timestamp"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""ChangedColumns"" TEXT NULL,
                        ""OldValues"" TEXT NULL,
                        ""NewValues"" TEXT NULL,
                        ""IpAddress"" VARCHAR(100) NULL,
                        ""CorrelationId"" UUID NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL
                    );

                    CREATE INDEX IF NOT EXISTS ""IX_AuditLogs_EntityName"" ON ""AuditLogs"" (""EntityName"");
                    CREATE INDEX IF NOT EXISTS ""IX_AuditLogs_EntityId"" ON ""AuditLogs"" (""EntityId"");
                    CREATE INDEX IF NOT EXISTS ""IX_AuditLogs_UserIdentity"" ON ""AuditLogs"" (""UserIdentity"");
                    CREATE INDEX IF NOT EXISTS ""IX_AuditLogs_Timestamp"" ON ""AuditLogs"" (""Timestamp"");
                    CREATE INDEX IF NOT EXISTS ""IX_AuditLogs_EntityName_Timestamp"" ON ""AuditLogs"" (""EntityName"", ""Timestamp"");

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RefreshTokens_Token"" ON ""RefreshTokens"" (""Token"");

                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""Token"" VARCHAR(250) NOT NULL DEFAULT '';
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""UsuarioId"" UUID NOT NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""ExpiresAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""IsRevoked"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""RevokedAt"" TIMESTAMP WITH TIME ZONE NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""CreatedByIp"" VARCHAR(50) NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""RevokedByIp"" VARCHAR(50) NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""ReplacedByToken"" VARCHAR(250) NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""ReasonRevoked"" VARCHAR(500) NULL;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE;
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    ALTER TABLE IF EXISTS ""RefreshTokens"" ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NULL;
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

            // Semeia e sincroniza Sacerdotes, Serviços de Consulta e Agendamentos completos
            await SeedAgendamentosAndSacerdotesAsync(cancellationToken);

            // Semeia os Usuários Iniciais do Sistema (Admin, Sacerdote, Secretaria, Consulente)
            await SeedUsuariosAsync(cancellationToken);

            _logger.LogInformation("SGAE Seeder: Carga inicial de dados finalizada com pleno sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "SGAE Seeder: Erro catastrófico ao inicializar e semear o banco de dados.");
            throw;
        }
    }

    private async Task SeedAgendamentosAndSacerdotesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 1. Assegura a presença do Sacerdote Padrão
            var sacerdoteDefaultId = Guid.Parse("16f372fa-5435-4b3e-a949-dd76dd98bf77");
            var sacerdote = await _context.Sacerdotes.FirstOrDefaultAsync(s => s.Nome.Contains("Sidnei"), cancellationToken);
            if (sacerdote == null)
            {
                sacerdote = new Sacerdote(
                    "Babalorixa Sidnei T' Sango",
                    "Jogo de Búzios e Orientação Espiritual de Tradição Nagô",
                    true
                );
                typeof(Sgae.Domain.Common.BaseEntity).GetProperty("Id")?.SetValue(sacerdote, sacerdoteDefaultId);
                await _context.Sacerdotes.AddAsync(sacerdote, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2. Assegura a presença do Serviço de Consulta Padrão
            var servicoDefaultId = Guid.Parse("fcd150b0-71a6-4067-b2f3-243a688c101d");
            var servico = await _context.ServicosConsulta.FirstOrDefaultAsync(s => s.Nome.Contains("Búzios"), cancellationToken);
            if (servico == null)
            {
                servico = new ServicoConsulta("Jogo de Búzios", 250.00m, true);
                typeof(Sgae.Domain.Common.BaseEntity).GetProperty("Id")?.SetValue(servico, servicoDefaultId);
                await _context.ServicosConsulta.AddAsync(servico, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 3. Atualiza agendamentos existentes que possuam campos nulos para refletirem dados completos e fiéis no grid
            var agendamentosIncompletos = await _context.Agendamentos
                .Where(a => a.SacerdoteId == null || a.ServicoConsultaId == null || a.FormaPagamento == null)
                .ToListAsync(cancellationToken);

            foreach (var ag in agendamentosIncompletos)
            {
                if (ag.SacerdoteId == null)
                {
                    ag.DefinirSacerdote(sacerdote.Id);
                }
                if (ag.ServicoConsultaId == null)
                {
                    ag.DefinirServicoConsulta(servico.Id);
                }
                if (string.IsNullOrEmpty(ag.FormaPagamento))
                {
                    ag.AtualizarDetalhes(
                        "Cartão",
                        ag.Pago,
                        ag.Observacoes ?? "",
                        ag.WhatsappConfirmacaoDisparada,
                        ag.ConfigLembrete ?? "Não Configurado",
                        ag.FrequenciaLembrete ?? "Nenhum"
                    );
                }
            }

            if (agendamentosIncompletos.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("SGAE Seeder: Sincronizados {Count} agendamentos com dados pastorais e financeiros completos.", agendamentosIncompletos.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SGAE Seeder: Aviso ao semear/sincronizar sacerdotes e agendamentos.");
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

    private async Task SeedUsuariosAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SGAE Seeder: Verificando tabela de usuários do sistema...");

        try
        {
            // Busca o sacerdote padrão para vincular à conta do sacerdote se existir
            var sacerdote = await _context.Sacerdotes.FirstOrDefaultAsync(s => s.Nome.Contains("Sidnei"), cancellationToken);

            // Busca o perfil de admin e pastoral staff se existirem
            var adminRole = await _context.PastoralRoles.FirstOrDefaultAsync(r => r.Nome == "Admin", cancellationToken);
            var pastorRole = await _context.PastoralRoles.FirstOrDefaultAsync(r => r.Nome == "Pastor", cancellationToken);
            var coordenadorRole = await _context.PastoralRoles.FirstOrDefaultAsync(r => r.Nome == "Coordenador", cancellationToken);

            var defaultUsers = new[]
            {
                (
                    Nome: "Administrador SGAE",
                    Email: "admin@sgae.com.br",
                    Senha: "Mudar@123",
                    Perfil: PerfilUsuario.Admin,
                    SacerdoteId: (Guid?)null,
                    RoleId: adminRole?.Id
                ),
                (
                    Nome: "Administrador SGAE",
                    Email: "admin@sgae.com",
                    Senha: "SgaeAdmin2026!",
                    Perfil: PerfilUsuario.Admin,
                    SacerdoteId: (Guid?)null,
                    RoleId: adminRole?.Id
                ),
                (
                    Nome: "Babalorixá Sidnei",
                    Email: "sacerdote@sgae.com.br",
                    Senha: "Mudar@123",
                    Perfil: PerfilUsuario.Sacerdote,
                    SacerdoteId: sacerdote?.Id,
                    RoleId: pastorRole?.Id
                ),
                (
                    Nome: "Babalorixá Sidnei",
                    Email: "sacerdote@sgae.com",
                    Senha: "SgaeSacerdote2026!",
                    Perfil: PerfilUsuario.Sacerdote,
                    SacerdoteId: sacerdote?.Id,
                    RoleId: pastorRole?.Id
                ),
                (
                    Nome: "Secretaria Pastoral",
                    Email: "secretaria@sgae.com.br",
                    Senha: "Mudar@123",
                    Perfil: PerfilUsuario.Secretaria,
                    SacerdoteId: (Guid?)null,
                    RoleId: coordenadorRole?.Id
                ),
                (
                    Nome: "Secretaria Pastoral",
                    Email: "secretaria@sgae.com",
                    Senha: "SgaeSecretaria2026!",
                    Perfil: PerfilUsuario.Secretaria,
                    SacerdoteId: (Guid?)null,
                    RoleId: coordenadorRole?.Id
                ),
                (
                    Nome: "Consulente Visitante",
                    Email: "consulente@sgae.com.br",
                    Senha: "Mudar@123",
                    Perfil: PerfilUsuario.Consulente,
                    SacerdoteId: (Guid?)null,
                    RoleId: (Guid?)null
                ),
                (
                    Nome: "Consulente Visitante",
                    Email: "consulente@sgae.com",
                    Senha: "SgaeConsulente2026!",
                    Perfil: PerfilUsuario.Consulente,
                    SacerdoteId: (Guid?)null,
                    RoleId: (Guid?)null
                )
            };

            var addedCount = 0;
            foreach (var userDef in defaultUsers)
            {
                var existing = await _context.Usuarios
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email == userDef.Email, cancellationToken);

                if (existing == null)
                {
                    var newUser = new Usuario(
                        userDef.Nome,
                        userDef.Email,
                        _passwordHasher.HashPassword(userDef.Senha),
                        userDef.Perfil,
                        userDef.SacerdoteId,
                        userDef.RoleId,
                        primeiroAcesso: false
                    );
                    await _context.Usuarios.AddAsync(newUser, cancellationToken);
                    addedCount++;
                }
                else
                {
                    var passwordMatches = _passwordHasher.VerifyPassword(userDef.Senha, existing.PasswordHash);
                    if (!passwordMatches)
                    {
                        existing.UpdatePassword(_passwordHasher.HashPassword(userDef.Senha));
                    }
                    if (!existing.StatusAtivo)
                    {
                        existing.SetStatus(true);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE Seeder: Sincronização de usuários padrão concluída com sucesso. Novos inseridos: {Count}", addedCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SGAE Seeder: Aviso ao semear usuários do sistema.");
        }
    }
}
