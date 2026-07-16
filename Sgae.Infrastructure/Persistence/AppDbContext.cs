using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Contexto principal de dados do SGAE. Configurado para PostgreSQL com Fluent API estrito.
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<PerfilConsulente> PerfisConsulentes => Set<PerfilConsulente>();
    public DbSet<AtendimentoEspiritual> AtendimentosEspirituais => Set<AtendimentoEspiritual>();
    public DbSet<Acompanhamento> Acompanhamentos => Set<Acompanhamento>();
    public DbSet<PastoralRole> PastoralRoles => Set<PastoralRole>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<SpiritualAttendanceCategory> SpiritualAttendanceCategories => Set<SpiritualAttendanceCategory>();
    public DbSet<CanalCaptacao> CanaisCaptacao => Set<CanalCaptacao>();
    public DbSet<Sacerdote> Sacerdotes => Set<Sacerdote>();
    public DbSet<ServicoConsulta> ServicosConsulta => Set<ServicoConsulta>();
    public DbSet<RitualSugerido> RituaisSugeridos => Set<RitualSugerido>();
    public DbSet<LocalRealizacao> LocaisRealizacao => Set<LocalRealizacao>();
    public DbSet<CategoriaInsumo> CategoriasInsumos => Set<CategoriaInsumo>();
    public DbSet<CategoriaDespesa> CategoriasDespesas => Set<CategoriaDespesa>();
    public DbSet<CustoInsumo> CustosInsumos => Set<CustoInsumo>();
    public DbSet<Prescricao> Prescricoes => Set<Prescricao>();
    public DbSet<Conversao> Conversoes => Set<Conversao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração Estrita da Entidade Lead (Captação)
        modelBuilder.Entity<Lead>(builder =>
        {
            builder.ToTable("Leads");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Nome)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(l => l.Telefone)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(l => l.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(l => l.Cidade)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(l => l.Estado)
                .HasMaxLength(2)
                .IsFixedLength()
                .IsRequired();

            builder.Property(l => l.DataContato)
                .IsRequired();

            // Mapeando Enum como String no PostgreSQL para segurança e legibilidade das queries externas
            builder.Property(l => l.Origem)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(l => l.ProblemaPrincipal)
                .HasMaxLength(1000)
                .IsRequired();

            builder.HasOne(l => l.CanalCaptacao)
                .WithMany(c => c.Leads)
                .HasForeignKey(l => l.CanalCaptacaoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Filtro Global para Soft Delete (IsDeleted == false)
            builder.HasQueryFilter(l => !l.IsDeleted);
        });

        // Configuração Estrita da Entidade Agendamento (Agendamento)
        modelBuilder.Entity<Agendamento>(builder =>
        {
            builder.ToTable("Agendamentos");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.DataHora)
                .IsRequired();

            builder.Property(a => a.Valor)
                .HasPrecision(18, 2)
                .IsRequired();

            // Mapeando Enums como String no banco
            builder.Property(a => a.Modalidade)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.MotivoCancelamento)
                .HasMaxLength(500)
                .IsRequired(false);

            // Relacionamento Fluente: 1 Lead para N Agendamentos
            builder.HasOne(a => a.Lead)
                .WithMany(l => l.Agendamentos)
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Restrict); // Evita delete em cascata acidental

            builder.HasOne(a => a.Sacerdote)
                .WithMany(s => s.Agendamentos)
                .HasForeignKey(a => a.SacerdoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ServicoConsulta)
                .WithMany(s => s.Agendamentos)
                .HasForeignKey(a => a.ServicoConsultaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(a => !a.IsDeleted);
        });

        // Configuração Estrita da Entidade PerfilConsulente (Etapa 3)
        modelBuilder.Entity<PerfilConsulente>(builder =>
        {
            builder.ToTable("PerfisConsulentes");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Idade)
                .IsRequired();

            builder.Property(p => p.FaixaEtaria)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Genero)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Profissao)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Escolaridade)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.EstadoCivil)
                .HasMaxLength(50)
                .IsRequired();

            // Relacionamento 1-para-1 entre Lead e PerfilConsulente
            builder.HasOne(p => p.Lead)
                .WithOne(l => l.Perfil)
                .HasForeignKey<PerfilConsulente>(p => p.LeadId)
                .OnDelete(DeleteBehavior.Cascade); // Se o Lead for removido, o perfil também é

            builder.HasQueryFilter(p => !p.IsDeleted);
        });

        // Configuração Estrita da Entidade AtendimentoEspiritual (Etapa 4)
        modelBuilder.Entity<AtendimentoEspiritual>(builder =>
        {
            builder.ToTable("AtendimentosEspirituais");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.TempoDuracaoMinutos)
                .IsRequired();

            builder.Property(a => a.TemasAbordados)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(a => a.Observacoes)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(a => a.Tipo)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // Relacionamento 1-para-1 entre Agendamento e AtendimentoEspiritual
            builder.HasOne(a => a.Agendamento)
                .WithOne(ag => ag.Atendimento)
                .HasForeignKey<AtendimentoEspiritual>(a => a.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade); // Se o agendamento for cancelado/expurgado de forma física

            builder.HasQueryFilter(a => !a.IsDeleted);
        });

        // Configuração Estrita da Entidade Acompanhamento
        modelBuilder.Entity<Acompanhamento>(builder =>
        {
            builder.ToTable("Acompanhamentos");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.DataAcompanhamento)
                .IsRequired();

            builder.Property(a => a.SintomasMelhora)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(a => a.Recomendacoes)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(a => a.Observacoes)
                .HasMaxLength(2000)
                .IsRequired();

            // Relacionamento: 1 AtendimentoEspiritual pode ter vários Acompanhamentos
            builder.HasOne(a => a.AtendimentoEspiritual)
                .WithMany()
                .HasForeignKey(a => a.AtendimentoEspiritualId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(a => !a.IsDeleted);
        });

        // Configuração Estrita da Entidade PastoralRole
        modelBuilder.Entity<PastoralRole>(builder =>
        {
            builder.ToTable("PastoralRoles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(r => r.EscopoPermissao)
                .HasMaxLength(250)
                .IsRequired();

            // Índice para busca e exclusividade do Nome de forma lógica
            builder.HasIndex(r => r.Nome)
                .IsUnique();

            builder.HasQueryFilter(r => !r.IsDeleted);
        });

        // Configuração Estrita da Entidade SystemConfiguration
        modelBuilder.Entity<SystemConfiguration>(builder =>
        {
            builder.ToTable("SystemConfigurations");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Chave)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(c => c.Valor)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(c => c.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(c => c.Chave)
                .IsUnique();

            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade SpiritualAttendanceCategory
        modelBuilder.Entity<SpiritualAttendanceCategory>(builder =>
        {
            builder.ToTable("SpiritualAttendanceCategories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Tipo)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(c => c.TempoRecomendadoMinutos)
                .IsRequired();

            builder.HasIndex(c => c.Tipo)
                .IsUnique();

            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade CanalCaptacao
        modelBuilder.Entity<CanalCaptacao>(builder =>
        {
            builder.ToTable("CanaisCaptacao");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(c => c.Nome).IsUnique();
            builder.Property(c => c.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade Sacerdote
        modelBuilder.Entity<Sacerdote>(builder =>
        {
            builder.ToTable("Sacerdotes");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(s => s.Nome).IsUnique();
            builder.Property(s => s.Especialidade).HasMaxLength(500).IsRequired(false);
            builder.Property(s => s.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(s => !s.IsDeleted);
        });

        // Configuração Estrita da Entidade ServicoConsulta
        modelBuilder.Entity<ServicoConsulta>(builder =>
        {
            builder.ToTable("ServicosConsulta");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(s => s.Nome).IsUnique();
            builder.Property(s => s.Tarifa).HasPrecision(18, 2).IsRequired();
            builder.Property(s => s.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(s => !s.IsDeleted);
        });

        // Configuração Estrita da Entidade RitualSugerido
        modelBuilder.Entity<RitualSugerido>(builder =>
        {
            builder.ToTable("RituaisSugeridos");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(r => r.Nome).IsUnique();
            builder.Property(r => r.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(r => !r.IsDeleted);
        });

        // Configuração Estrita da Entidade LocalRealizacao
        modelBuilder.Entity<LocalRealizacao>(builder =>
        {
            builder.ToTable("LocaisRealizacao");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(l => l.Nome).IsUnique();
            builder.Property(l => l.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(l => !l.IsDeleted);
        });

        // Configuração Estrita da Entidade CategoriaInsumo
        modelBuilder.Entity<CategoriaInsumo>(builder =>
        {
            builder.ToTable("CategoriasInsumos");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(c => c.Nome).IsUnique();
            builder.Property(c => c.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade CategoriaDespesa
        modelBuilder.Entity<CategoriaDespesa>(builder =>
        {
            builder.ToTable("CategoriasDespesas");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            builder.HasIndex(c => c.Nome).IsUnique();
            builder.Property(c => c.Ativo).IsRequired().HasDefaultValue(true);
            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade CustoInsumo
        modelBuilder.Entity<CustoInsumo>(builder =>
        {
            builder.ToTable("CustosInsumos");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Descricao).HasMaxLength(500).IsRequired();
            builder.Property(c => c.Valor).HasPrecision(18, 2).IsRequired();

            builder.HasOne(c => c.CategoriaInsumo)
                .WithMany(i => i.Custos)
                .HasForeignKey(c => c.CategoriaInsumoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CategoriaDespesa)
                .WithMany(d => d.Custos)
                .HasForeignKey(c => c.CategoriaDespesaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configuração Estrita da Entidade Prescricao
        modelBuilder.Entity<Prescricao>(builder =>
        {
            builder.ToTable("Prescricoes");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Descricao).HasMaxLength(1000).IsRequired();

            builder.HasOne(p => p.Sacerdote)
                .WithMany(s => s.Prescricoes)
                .HasForeignKey(p => p.SacerdoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(p => !p.IsDeleted);
        });

        // Configuração Estrita da Entidade Conversao
        modelBuilder.Entity<Conversao>(builder =>
        {
            builder.ToTable("Conversoes");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Descricao).HasMaxLength(1000).IsRequired();

            builder.HasOne(c => c.Sacerdote)
                .WithMany(s => s.Conversoes)
                .HasForeignKey(c => c.SacerdoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(c => !c.IsDeleted);
        });

        // Conversor global de DateTime para UTC para evitar problemas de fuso horário / Unspecified com o PostgreSQL (Npgsql)
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : (v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc)),
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue ? null : (v.Value.Kind == DateTimeKind.Utc ? v : (v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))),
            v => !v.HasValue ? null : (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}