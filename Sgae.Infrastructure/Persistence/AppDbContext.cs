using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Application.Abstractions;

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
    }
}