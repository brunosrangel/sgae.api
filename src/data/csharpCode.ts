export interface FileNode {
  name: string;
  type: 'file' | 'folder';
  path: string;
  content?: string;
  language?: string;
  description?: string;
  children?: FileNode[];
}

export const cleanArchitectureDescription = {
  domain: "A camada de Domínio é o coração da aplicação. Contém as Entidades de Negócio, Objetos de Valor (Value Objects), Enums, Exceções de Domínio e interfaces dos Repositórios. É totalmente isolada de frameworks, bancos de dados ou qualquer dependência externa (independência tecnológica total).",
  application: "A camada de Aplicação implementa os Casos de Uso do sistema. Aqui reside a orquestração CQRS (Commands e Queries), handlers do MediatR, validações via FluentValidation e mapeamentos. Depende apenas do Domínio.",
  infrastructure: "A camada de Infraestrutura gerencia as preocupações técnicas do sistema. Implementa a persistência (Entity Framework Core, Dapper), configurações do DbContext, migrações do PostgreSQL, integrações de APIs externas, mensageria e repositórios concretos.",
  api: "A camada de Apresentação (Presentation / Web API) expõe os endpoints RESTful para o mundo externo. Contém os Controllers ou Minimal APIs, configurações de DI (Injeção de Dependências), Swagger, autenticação via JWT e middlewares globais (como tratamento de erros)."
};

export const sgaeFileStructure: FileNode[] = [
  {
    name: "SgaeSolution.sln",
    type: "file",
    path: "SgaeSolution.sln",
    language: "xml",
    description: "Arquivo de solução do Visual Studio / .NET que agrupa todos os subprojetos da arquitetura limpa.",
    content: `Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.8.34309.116
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sgae.Domain", "Sgae.Domain\\Sgae.Domain.csproj", "{D1C1B1A1-2C22-4D33-8E44-9F55A66BCC11}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sgae.Application", "Sgae.Application\\Sgae.Application.csproj", "{A1C1B1A5-3C22-4D33-8E44-9F55A66BCC22}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sgae.Infrastructure", "Sgae.Infrastructure\\Sgae.Infrastructure.csproj", "{I1C1B1A7-4C22-4D33-8E44-9F55A66BCC33}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sgae.API", "Sgae.API\\Sgae.API.csproj", "{P1C1B1A9-5C22-4D33-8E44-9F55A66BCC44}"
EndProject`
  },
  {
    name: "Sgae.Domain",
    type: "folder",
    path: "Sgae.Domain",
    children: [
      {
        name: "Exceptions",
        type: "folder",
        path: "Sgae.Domain/Exceptions",
        children: [
          {
            name: "DomainException.cs",
            type: "file",
            path: "Sgae.Domain/Exceptions/DomainException.cs",
            language: "csharp",
            description: "Classe de exceção base para regras de negócio e violações de invariantes de domínio.",
            content: `using System;

namespace Sgae.Domain.Exceptions;

/// <summary>
/// Classe de exceção base para regras de negócio e violações de invariantes de domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}`
          }
        ]
      },
      {
        name: "Common",
        type: "folder",
        path: "Sgae.Domain/Common",
        children: [
          {
            name: "BaseEntity.cs",
            type: "file",
            path: "Sgae.Domain/Common/BaseEntity.cs",
            language: "csharp",
            description: "Classe abstrata base para fornecer propriedades comuns como Id autogerado, auditoria (CreatedAt, UpdatedAt) e controle de exclusão lógica.",
            content: `using System;

namespace Sgae.Domain.Common;

/// <summary>
/// Classe abstrata base para todas as entidades de domínio com suporte à exclusão lógica e auditoria básica.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public void RegisterUpdate()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        RegisterUpdate();
    }
}`
          }
        ]
      },
      {
        name: "Entities",
        type: "folder",
        path: "Sgae.Domain/Entities",
        children: [
          {
            name: "Lead.cs",
            type: "file",
            path: "Sgae.Domain/Entities/Lead.cs",
            language: "csharp",
            description: "Entidade Rica de Domínio representando a Etapa 1 - Captação. Possui encapsulated setters para garantir integridade e regras de negócio.",
            content: `using System;
using System.Collections.Generic;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando o Consulente na etapa de Captação (Lead).
/// </summary>
public class Lead : BaseEntity
{
    // Construtor privado para o EF Core
    private Lead() { }

    public Lead(
        string nome, 
        string telefone, 
        string email, 
        string cidade, 
        string estado, 
        OrigemContato origem, 
        string problemaPrincipal)
    {
        UpdateDadosPessoais(nome, email, telefone);
        UpdateLocalizacao(cidade, estado);
        Origem = origem;
        ProblemaPrincipal = problemaPrincipal ?? throw new ArgumentException("O problema principal deve ser especificado para captação.");
        DataContato = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public DateTime DataContato { get; private set; }
    public OrigemContato Origem { get; private set; }
    public string ProblemaPrincipal { get; private set; } = null!;

    // Relacionamento de Navegação (1-para-N): Um Lead pode ter vários agendamentos no sistema
    public virtual ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();

    // Relacionamento de Navegação 1-para-1 com o Perfil do Consulente (Etapa 3 - Perfil)
    public virtual PerfilConsulente? Perfil { get; private set; }

    // Métodos de Negócio (Ricos / DDD)
    public void UpdateDadosPessoais(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.");
        
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone para contato é obrigatório.");

        Nome = nome.Trim();
        Email = email?.Trim() ?? string.Empty;
        Telefone = telefone.Trim();
        RegisterUpdate();
    }

    public void UpdateLocalizacao(string cidade, string estado)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade é obrigatória.");
        
        if (string.IsNullOrWhiteSpace(estado) || estado.Length != 2)
            throw new ArgumentException("Estado é obrigatório e deve conter exatamente 2 caracteres (UF).");

        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpper();
        RegisterUpdate();
    }

    public void AlterarProblemaPrincipal(string novoProblema)
    {
        if (string.IsNullOrWhiteSpace(novoProblema))
            throw new ArgumentException("O problema principal não pode ser nulo.");
        
        ProblemaPrincipal = novoProblema.Trim();
        RegisterUpdate();
    }

    public void DefinirPerfil(PerfilConsulente perfil)
    {
        Perfil = perfil ?? throw new ArgumentNullException(nameof(perfil));
        RegisterUpdate();
    }
}`
          },
          {
            name: "PerfilConsulente.cs",
            type: "file",
            path: "Sgae.Domain/Entities/PerfilConsulente.cs",
            language: "csharp",
            description: "Entidade Rica de Domínio representando a Etapa 3 - Perfil do Consulente com detalhes sócio-demográficos.",
            content: `using System;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando a Etapa 3 - Perfil do Consulente contendo dados demográficos adicionais.
/// </summary>
public class PerfilConsulente : BaseEntity
{
    private PerfilConsulente() { }

    public PerfilConsulente(
        Guid leadId,
        int idade,
        string faixaEtaria,
        string genero,
        string profissao,
        string escolaridade,
        string estadoCivil)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("O perfil deve estar vinculado a um Consulente (LeadId) válido.");

        if (idade <= 0 || idade > 120)
            throw new ArgumentException("A idade informada deve ser válida (entre 1 e 120 anos).");

        LeadId = leadId;
        UpdatePerfil(idade, faixaEtaria, genero, profissao, escolaridade, estadoCivil);
    }

    public Guid LeadId { get; private set; }
    public virtual Lead Lead { get; private set; } = null!;

    public int Idade { get; private set; }
    public string FaixaEtaria { get; private set; } = null!;
    public string Genero { get; private set; } = null!;
    public string Profissao { get; private set; } = null!;
    public string Escolaridade { get; private set; } = null!;
    public string EstadoCivil { get; private set; } = null!;

    public void UpdatePerfil(
        int idade,
        string faixaEtaria,
        string genero,
        string profissao,
        string escolaridade,
        string estadoCivil)
    {
        if (idade <= 0 || idade > 120)
            throw new ArgumentException("Idade deve ser entre 1 e 120 anos.");

        if (string.IsNullOrWhiteSpace(faixaEtaria))
            throw new ArgumentException("Faixa etária é obrigatória.");

        if (string.IsNullOrWhiteSpace(genero))
            throw new ArgumentException("Gênero é obrigatório.");

        if (string.IsNullOrWhiteSpace(profissao))
            throw new ArgumentException("Profissão é obrigatória.");

        if (string.IsNullOrWhiteSpace(escolaridade))
            throw new ArgumentException("Escolaridade é obrigatória.");

        if (string.IsNullOrWhiteSpace(estadoCivil))
            throw new ArgumentException("Estado civil é obrigatório.");

        Idade = idade;
        FaixaEtaria = faixaEtaria.Trim();
        Genero = genero.Trim();
        Profissao = profissao.Trim();
        Escolaridade = escolaridade.Trim();
        EstadoCivil = estadoCivil.Trim();
        RegisterUpdate();
    }
}`
          },
          {
            name: "Agendamento.cs",
            type: "file",
            path: "Sgae.Domain/Entities/Agendamento.cs",
            language: "csharp",
            description: "Entidade Rica de Domínio representando a Etapa 2 - Agendamento. Controla transições de status e regras de preço e cancelamento.",
            content: `using System;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando o Agendamento de uma consulta espiritual associada a um Lead/Consulente.
/// </summary>
public class Agendamento : BaseEntity
{
    private Agendamento() { }

    public Agendamento(
        Guid leadId,
        DateTime dataHora, 
        ModalidadeAtendimento modalidade, 
        decimal valor)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("O agendamento deve estar associado a um consulente (LeadId) válido.");

        if (dataHora < DateTime.UtcNow)
            throw new ArgumentException("A data do agendamento não pode ser retroativa.");

        if (valor < 0)
            throw new ArgumentException("O valor do agendamento não pode ser negativo.");

        LeadId = leadId;
        DataHora = dataHora;
        Modalidade = modalidade;
        Valor = valor;
        Status = StatusAgendamento.Pendente;
        MotivoCancelamento = null;
    }

    public Guid LeadId { get; private set; }
    public virtual Lead Lead { get; private set; } = null!;

    public DateTime DataHora { get; private set; }
    public ModalidadeAtendimento Modalidade { get; private set; }
    public decimal Valor { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    // Métodos de Regras de Negócio (Status State Transitions)
    public void ConfirmarAgendamento()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new InvalidOperationException($"Não é possível confirmar um agendamento com status atual: {Status}");

        Status = StatusAgendamento.Confirmado;
        RegisterUpdate();
    }

    public void RealizarAgendamento()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new InvalidOperationException("Apenas agendamentos Confirmados podem ser marcados como Realizados.");

        Status = StatusAgendamento.Realizado;
        RegisterUpdate();
    }

    public void CancelarAgendamento(string motivo)
    {
        if (Status == StatusAgendamento.Realizado)
            throw new InvalidOperationException("Não é possível cancelar um agendamento que já foi realizado.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("O motivo do cancelamento deve ser obrigatoriamente justificado.");

        Status = StatusAgendamento.Cancelado;
        MotivoCancelamento = motivo.Trim();
        RegisterUpdate();
    }

    public void MarcarComoAusente()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new InvalidOperationException("Apenas agendamentos Confirmados podem registrar ausência (no-show).");

        Status = StatusAgendamento.Ausente;
        RegisterUpdate();
    }

    public void Reagendar(DateTime novaDataHora)
    {
        if (novaDataHora < DateTime.UtcNow)
            throw new ArgumentException("Nova data de reagendamento não pode ser menor que a data/hora atual.");

        if (Status == StatusAgendamento.Realizado || Status == StatusAgendamento.Cancelado)
            throw new InvalidOperationException("Não é possível reagendar atendimentos concluídos ou cancelados.");

        DataHora = novaDataHora;
        Status = StatusAgendamento.Pendente; // Volta a requerer confirmação
        RegisterUpdate();
    }
}`
          }
        ]
      },
      {
        name: "Enums",
        type: "folder",
        path: "Sgae.Domain/Enums",
        children: [
          {
            name: "OrigemContato.cs",
            type: "file",
            path: "Sgae.Domain/Enums/OrigemContato.cs",
            language: "csharp",
            description: "Enumeração das origens de contato para a captação do consulente.",
            content: `namespace Sgae.Domain.Enums;

public enum OrigemContato
{
    WhatsApp = 1,
    Instagram = 2,
    Facebook = 3,
    GoogleSearch = 4,
    Indicacao = 5,
    SiteSgae = 6,
    Outros = 7
}`
          },
          {
            name: "ModalidadeAtendimento.cs",
            type: "file",
            path: "Sgae.Domain/Enums/ModalidadeAtendimento.cs",
            language: "csharp",
            description: "Modalidade em que a orientação ou consulta espiritual será ministrada.",
            content: `namespace Sgae.Domain.Enums;

public enum ModalidadeAtendimento
{
    Presencial = 1,
    Online = 2
}`
          },
          {
            name: "StatusAgendamento.cs",
            type: "file",
            path: "Sgae.Domain/Enums/StatusAgendamento.cs",
            language: "csharp",
            description: "Máquina de estados simplificada para ciclo de vida dos atendimentos.",
            content: `namespace Sgae.Domain.Enums;

public enum StatusAgendamento
{
    Pendente = 1,
    Confirmado = 2,
    Realizado = 3,
    Cancelado = 4,
    Ausente = 5
}`
          }
        ]
      }
    ]
  },
  {
    name: "Sgae.Infrastructure",
    type: "folder",
    path: "Sgae.Infrastructure",
    children: [
      {
        name: "Persistence",
        type: "folder",
        path: "Sgae.Infrastructure/Persistence",
        children: [
          {
            name: "AppDbContext.cs",
            type: "file",
            path: "Sgae.Infrastructure/Persistence/AppDbContext.cs",
            language: "csharp",
            description: "Contexto de persistência do EF Core configurado com convenções robustas do PostgreSQL para as etapas 1 e 2.",
            content: `using Microsoft.EntityFrameworkCore;
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
}`
          },
          {
            name: "UnitOfWork.cs",
            type: "file",
            path: "Sgae.Infrastructure/Persistence/UnitOfWork.cs",
            language: "csharp",
            description: "Implementação concreta do padrão Unit of Work delegando as transações do MediatR ao DbContext do EF Core para manter atomicidade.",
            content: `using System.Threading;
using System.Threading.Tasks;
using Sgae.Application.Abstractions;

namespace Sgae.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}`
          }
        ]
      }
    ]
  },
  {
    name: "Sgae.Application",
    type: "folder",
    path: "Sgae.Application",
    children: [
      {
        name: "Abstractions",
        type: "folder",
        path: "Sgae.Application/Abstractions",
        children: [
          {
            name: "IAppDbContext.cs",
            type: "file",
            path: "Sgae.Application/Abstractions/IAppDbContext.cs",
            language: "csharp",
            description: "Interface que expõe apenas os DbSets de persistência seguros para a camada de Application, mantendo desacoplamento de infra.",
            content: `using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;

namespace Sgae.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Lead> Leads { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<PerfilConsulente> PerfisConsulentes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}`
          },
          {
            name: "IUnitOfWork.cs",
            type: "file",
            path: "Sgae.Application/Abstractions/IUnitOfWork.cs",
            language: "csharp",
            description: "Interface que expõe o padrão Unit of Work para persistência segura e atômica.",
            content: `using System.Threading;
using System.Threading.Tasks;

namespace Sgae.Application.Abstractions;

/// <summary>
/// Contrato do Unit of Work para persistir modificações de forma transacional e atômica.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}`
          }
        ]
      },
      {
        name: "Leads",
        type: "folder",
        path: "Sgae.Application/Leads",
        children: [
          {
            name: "DTOs",
            type: "folder",
            path: "Sgae.Application/Leads/DTOs",
            children: [
              {
                name: "LeadDto.cs",
                type: "file",
                path: "Sgae.Application/Leads/DTOs/LeadDto.cs",
                language: "csharp",
                description: "Objeto de Transferência de Dados do Consulente (Lead) para desacoplar o modelo de apresentação do domínio.",
                content: `using System;

namespace Sgae.Application.Leads.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string ProblemaPrincipal { get; set; } = string.Empty;
    public DateTime DataCaptacao { get; set; }
}`
              }
            ]
          },
          {
            name: "Commands",
            type: "folder",
            path: "Sgae.Application/Leads/Commands",
            children: [
              {
                name: "CreateLead",
                type: "folder",
                path: "Sgae.Application/Leads/Commands/CreateLead",
                children: [
                  {
                    name: "CreateLeadCommand.cs",
                    type: "file",
                    path: "Sgae.Application/Leads/Commands/CreateLead/CreateLeadCommand.cs",
                    language: "csharp",
                    description: "Comando imutável para a captação do Consulente.",
                    content: `using System;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Comando contendo os parâmetros de entrada requeridos para a captação de um novo Consulente (Lead).
/// </summary>
public record CreateLeadCommand(
    string Nome,
    string Telefone,
    string Email,
    string Cidade,
    string Estado,
    OrigemContato Origem,
    string ProblemaPrincipal
) : ICommand<Guid>;`
                  },
                  {
                    name: "CreateLeadCommandHandler.cs",
                    type: "file",
                    path: "Sgae.Application/Leads/Commands/CreateLead/CreateLeadCommandHandler.cs",
                    language: "csharp",
                    description: "Tratador/Handler do comando de captação que invoca as invariantes de domínio e faz a gravação.",
                    content: `using System;
using System.Threading;
using System.Threading.Tasks;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Manipulador que recebe o comando, cria a entidade rica e persiste via Unit of Work.
/// </summary>
public class CreateLeadCommandHandler : ICommandHandler<CreateLeadCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        // Instanciação rica da entidade de domínio (validará as exigências e invariantes internamente)
        var lead = new Lead(
            request.Nome,
            request.Telefone,
            request.Email,
            request.Cidade,
            request.Estado,
            request.Origem,
            request.ProblemaPrincipal
        );

        await _context.Leads.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}`
                  },
                  {
                    name: "CreateLeadCommandValidator.cs",
                    type: "file",
                    path: "Sgae.Application/Leads/Commands/CreateLead/CreateLeadCommandValidator.cs",
                    language: "csharp",
                    description: "Validador FluentValidation do comando de criação de Lead, validando campos de captação essenciais.",
                    content: `using FluentValidation;

namespace Sgae.Application.Leads.Commands.CreateLead;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(v => v.Nome)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(150).WithMessage("Nome não pode exceder 150 caracteres.");

        RuleFor(v => v.Telefone)
            .NotEmpty().WithMessage("Telefone para contato é obrigatório.")
            .MaximumLength(25).WithMessage("Telefone não pode exceder 25 caracteres.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(v => v.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória.");

        RuleFor(v => v.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve conter exatamente 2 caracteres (UF).");

        RuleFor(v => v.ProblemaPrincipal)
            .NotEmpty().WithMessage("O problema principal deve ser especificado para captação.");
    }
}`
                  }
                ]
              }
            ]
          },
          {
            name: "Queries",
            type: "folder",
            path: "Sgae.Application/Leads/Queries",
            children: [
              {
                name: "GetLeadsWithPagination",
                type: "folder",
                path: "Sgae.Application/Leads/Queries/GetLeadsWithPagination",
                children: [
                  {
                    name: "GetLeadsWithPaginationQuery.cs",
                    type: "file",
                    path: "Sgae.Application/Leads/Queries/GetLeadsWithPagination/GetLeadsWithPaginationQuery.cs",
                    language: "csharp",
                    description: "Consulta paginada focada em recuperar Leads de forma filtrada e performática.",
                    content: `using System;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadsWithPagination;

public record GetLeadsWithPaginationQuery : IQuery<PaginatedList<LeadDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
}`
                  },
                  {
                    name: "GetLeadsWithPaginationQueryHandler.cs",
                    type: "file",
                    path: "Sgae.Application/Leads/Queries/GetLeadsWithPagination/GetLeadsWithPaginationQueryHandler.cs",
                    language: "csharp",
                    description: "Tratador/Handler do comando de consulta que recupera do banco aplicando filtros e projetando com AutoMapper.",
                    content: `using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Common.Models;
using Sgae.Application.Leads.DTOs;

namespace Sgae.Application.Leads.Queries.GetLeadsWithPagination;

public class GetLeadsWithPaginationQueryHandler : IQueryHandler<GetLeadsWithPaginationQuery, PaginatedList<LeadDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetLeadsWithPaginationQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<LeadDto>> Handle(GetLeadsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leads.AsNoTracking();

        // 1. Aplica filtros dinâmicos
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(l => l.Nome.ToLower().Contains(search) || 
                                     l.Email.ToLower().Contains(search) || 
                                     l.Cidade.ToLower().Contains(search));
        }

        if (request.DataInicio.HasValue)
        {
            query = query.Where(l => l.DataCaptacao >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            query = query.Where(l => l.DataCaptacao <= request.DataFim.Value);
        }

        // 2. Ordenação padrão por data de captação decrescente
        query = query.OrderByDescending(l => l.DataCaptacao);

        // 3. Projeta diretamente em DTO usando AutoMapper para otimização de Select (QueryableExtensions)
        return await PaginatedList<LeadDto>.CreateAsync(
            query.ProjectTo<LeadDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );
    }
}`
                  }
                ]
              }
            ]
          }
        ]
      },
      {
        name: "Agendamentos",
        type: "folder",
        path: "Sgae.Application/Agendamentos",
        children: [
          {
            name: "DTOs",
            type: "folder",
            path: "Sgae.Application/Agendamentos/DTOs",
            children: [
              {
                name: "AgendamentoDto.cs",
                type: "file",
                path: "Sgae.Application/Agendamentos/DTOs/AgendamentoDto.cs",
                language: "csharp",
                description: "Objeto de Transferência de Dados (DTO) para expor as consultas espirituais, incluindo dados básicos do Consulente.",
                content: `using System;

namespace Sgae.Application.Agendamentos.DTOs;

public class AgendamentoDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string LeadNome { get; set; } = string.Empty;
    public string LeadTelefone { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string Modalidade { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}`
              }
            ]
          },
          {
            name: "Commands",
            type: "folder",
            path: "Sgae.Application/Agendamentos/Commands",
            children: [
              {
                name: "CreateAgendamento",
                type: "folder",
                path: "Sgae.Application/Agendamentos/Commands/CreateAgendamento",
                children: [
                  {
                    name: "CreateAgendamentoCommand.cs",
                    type: "file",
                    path: "Sgae.Application/Agendamentos/Commands/CreateAgendamento/CreateAgendamentoCommand.cs",
                    language: "csharp",
                    description: "Comando de solicitação do agendamento de uma consulta vinculada a um Consulente existente.",
                    content: `using System;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Comando contendo as especificações do agendamento da consulta espiritual.
/// </summary>
public record CreateAgendamentoCommand(
    Guid LeadId,
    DateTime DataHora,
    ModalidadeAtendimento Modalidade,
    decimal Valor
) : ICommand<Guid>;`
                  },
                  {
                    name: "CreateAgendamentoCommandHandler.cs",
                    type: "file",
                    path: "Sgae.Application/Agendamentos/Commands/CreateAgendamento/CreateAgendamentoCommandHandler.cs",
                    language: "csharp",
                    description: "Tratador/Handler do comando de agendamento que garante a existência do Lead e as restrições temporais de negócio.",
                    content: `using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de criação de um agendamento espiritual.
/// </summary>
public class CreateAgendamentoCommandHandler : ICommandHandler<CreateAgendamentoCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgendamentoCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAgendamentoCommand request, CancellationToken cancellationToken)
    {
        // 1. Garante a integridade lógica: Consulente deve existir previamente na base
        var leadExists = await _context.Leads
            .AnyAsync(l => l.Id == request.LeadId, cancellationToken);

        if (!leadExists)
        {
            throw new ArgumentException($"O consulente de ID '{request.LeadId}' não foi localizado no sistema.");
        }

        // 2. Instancia a entidade rica executando as regras de estado
        var agendamento = new Agendamento(
            request.LeadId,
            request.DataHora,
            request.Modalidade,
            request.Valor
        );

        await _context.Agendamentos.AddAsync(agendamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}`
                  },
                  {
                    name: "CreateAgendamentoCommandValidator.cs",
                    type: "file",
                    path: "Sgae.Application/Agendamentos/Commands/CreateAgendamento/CreateAgendamentoCommandValidator.cs",
                    language: "csharp",
                    description: "Validador FluentValidation do comando de criação de agendamento, validando regras de data futura, valor correto e integridade de ID.",
                    content: `using System;
using FluentValidation;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

public class CreateAgendamentoCommandValidator : AbstractValidator<CreateAgendamentoCommand>
{
    public CreateAgendamentoCommandValidator()
    {
        RuleFor(v => v.LeadId)
            .NotEmpty().WithMessage("O consulente associado (LeadId) é obrigatório.");

        RuleFor(v => v.DataHora)
            .NotEmpty().WithMessage("A data do agendamento é obrigatória.")
            .GreaterThan(DateTime.UtcNow).WithMessage("A data de agendamento deve ser uma data futura.");

        RuleFor(v => v.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do agendamento não pode ser negativo.");
    }
}`
                  }
                ]
              }
            ]
          },
          {
            name: "Queries",
            type: "folder",
            path: "Sgae.Application/Agendamentos/Queries",
            children: [
              {
                name: "GetAgendamentosWithFilters",
                type: "folder",
                path: "Sgae.Application/Agendamentos/Queries/GetAgendamentosWithFilters",
                children: [
                  {
                    name: "GetAgendamentosWithFiltersQuery.cs",
                    type: "file",
                    path: "Sgae.Application/Agendamentos/Queries/GetAgendamentosWithFilters/GetAgendamentosWithFiltersQuery.cs",
                    language: "csharp",
                    description: "Consulta de agendamentos estruturada com filtros flexíveis de data, modalidade e status.",
                    content: `using System;
using System.Collections.Generic;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentosWithFilters;

public record GetAgendamentosWithFiltersQuery : IQuery<List<AgendamentoDto>>
{
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public ModalidadeAtendimento? Modalidade { get; init; }
    public StatusAgendamento? Status { get; init; }
}`
                  },
                  {
                    name: "GetAgendamentosWithFiltersQueryHandler.cs",
                    type: "file",
                    path: "Sgae.Application/Agendamentos/Queries/GetAgendamentosWithFilters/GetAgendamentosWithFiltersQueryHandler.cs",
                    language: "csharp",
                    description: "Tratador/Handler especializado em carregar agendamentos em lote aplicando projeção rápida com AutoMapper.",
                    content: `using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Agendamentos.DTOs;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentosWithFilters;

public class GetAgendamentosWithFiltersQueryHandler : IQueryHandler<GetAgendamentosWithFiltersQuery, List<AgendamentoDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAgendamentosWithFiltersQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AgendamentoDto>> Handle(GetAgendamentosWithFiltersQuery request, CancellationToken cancellationToken)
    {
        // Carrega as entidades de forma otimizada com Includes e projeta diretamente
        var query = _context.Agendamentos
            .Include(a => a.Lead) // Garante o carregamento dos dados do Consulente para mapeamento legal
            .AsNoTracking();

        // Aplicando os filtros inteligentes
        if (request.DataInicio.HasValue)
        {
            query = query.Where(a => a.DataHora >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            query = query.Where(a => a.DataHora <= request.DataFim.Value);
        }

        if (request.Modalidade.HasValue)
        {
            query = query.Where(a => a.Modalidade == request.Modalidade.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // Ordenado cronologicamente
        query = query.OrderBy(a => a.DataHora);

        // Retorna a projeção limpa via AutoMapper
        return await query
            .ProjectTo<AgendamentoDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}`
                  }
                ]
              }
            ]
          }
        ]
      },
      {
        name: "Common",
        type: "folder",
        path: "Sgae.Application/Common",
        children: [
          {
            name: "CQRS",
            type: "folder",
            path: "Sgae.Application/Common/CQRS",
            children: [
              {
                name: "ICommand.cs",
                type: "file",
                path: "Sgae.Application/Common/CQRS/ICommand.cs",
                language: "csharp",
                description: "Interface base para todos os Comandos CQRS que geram mutação de estado. Mapeia para o IRequest correspondente do MediatR.",
                content: `using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato base de CQRS para Commands que retornam um tipo de dado específico (ex: Guid do registro criado).
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Contrato base de CQRS para Commands sem retorno explícito (Void).
/// </summary>
public interface ICommand : IRequest
{
}`
              },
              {
                name: "IQuery.cs",
                type: "file",
                path: "Sgae.Application/Common/CQRS/IQuery.cs",
                language: "csharp",
                description: "Interface base para todas as Consultas CQRS focadas em leitura limpa e rápida do estado.",
                content: `using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato de CQRS para consultas (Query) que obrigatoriamente retornam um dado do tipo TResponse.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}`
              },
              {
                name: "ICommandHandler.cs",
                type: "file",
                path: "Sgae.Application/Common/CQRS/ICommandHandler.cs",
                language: "csharp",
                description: "Manipulador de Comandos tipado (CommandHandler) que orquestra a lógica de gravação e as regras de negócio.",
                content: `using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato para manipuladores de Comandos que possuem resposta do tipo TResponse.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Contrato para manipuladores de Comandos que não retornam dados.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}`
              },
              {
                name: "IQueryHandler.cs",
                type: "file",
                path: "Sgae.Application/Common/CQRS/IQueryHandler.cs",
                language: "csharp",
                description: "Manipulador de Consultas tipado (QueryHandler) especializado em leitura otimizada e mapeamento de DTOs.",
                content: `using MediatR;

namespace Sgae.Application.Common.CQRS;

/// <summary>
/// Contrato para manipuladores de consultas CQRS.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}`
              }
            ]
          },
          {
            name: "Models",
            type: "folder",
            path: "Sgae.Application/Common/Models",
            children: [
              {
                name: "PaginatedList.cs",
                type: "file",
                path: "Sgae.Application/Common/Models/PaginatedList.cs",
                language: "csharp",
                description: "Classe de paginação genérica e imutável para transporte seguro de coleções paginadas na API.",
                content: `using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Sgae.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}`
              }
            ]
          },
          {
            name: "Mappings",
            type: "folder",
            path: "Sgae.Application/Common/Mappings",
            children: [
              {
                name: "MappingProfile.cs",
                type: "file",
                path: "Sgae.Application/Common/Mappings/MappingProfile.cs",
                language: "csharp",
                description: "Profile do AutoMapper que define os mapas e as projeções customizadas entre entidades e DTOs.",
                content: `using AutoMapper;
using Sgae.Application.Leads.DTOs;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Domain.Entities;

namespace Sgae.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamento bidirecional ou unidirecional de Lead para LeadDto
        CreateMap<Lead, LeadDto>()
            .ForMember(dest => dest.Origem, opt => opt.MapFrom(src => src.Origem.ToString()));

        // Mapeamento enriquecido de Agendamento buscando campos da entidade navegacional 'Lead'
        CreateMap<Agendamento, AgendamentoDto>()
            .ForMember(dest => dest.LeadNome, opt => opt.MapFrom(src => src.Lead != null ? src.Lead.Nome : string.Empty))
            .ForMember(dest => dest.LeadTelefone, opt => opt.MapFrom(src => src.Lead != null ? src.Lead.Telefone : string.Empty))
            .ForMember(dest => dest.Modalidade, opt => opt.MapFrom(src => src.Modalidade.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}`
              }
            ]
          },
          {
            name: "Behaviors",
            type: "folder",
            path: "Sgae.Application/Common/Behaviors",
            children: [
              {
                name: "LoggingBehavior.cs",
                type: "file",
                path: "Sgae.Application/Common/Behaviors/LoggingBehavior.cs",
                language: "csharp",
                description: "IPipelineBehavior do MediatR para logar execuções de comandos/consultas de forma genérica, monitorando tempo de CPU e capturando erros.",
                content: `using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Sgae.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("SGAE Pipeline: Iniciando processamento do Request {RequestName} {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation("SGAE Pipeline: Finalizado processamento de {RequestName} com sucesso em {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SGAE Pipeline: Falha crítica na execução do Request {RequestName} após {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}`
              },
              {
                name: "ValidationBehavior.cs",
                type: "file",
                path: "Sgae.Application/Common/Behaviors/ValidationBehavior.cs",
                language: "csharp",
                description: "IPipelineBehavior do MediatR que intercepta comandos e executa validadores de forma transacional, lançando exceção se houver impedimentos.",
                content: `using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Sgae.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}`
              }
            ]
          }
        ]
      },
      {
        name: "DependencyInjection.cs",
        type: "file",
        path: "Sgae.Application/DependencyInjection.cs",
        language: "csharp",
        description: "Classe de injeção de dependência do Sgae.Application, configurando o MediatR e AutoMapper no container de DI.",
        content: `using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Sgae.Application.Common.Behaviors;

namespace Sgae.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra as dependências internas da camada Sgae.Application no contêiner de DI do .NET 8.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra o AutoMapper escaneando os profiles de mapeamento do Assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Registra todos os validadores do FluentValidation no Assembly atual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Registra o MediatR escaneando o Assembly atual da aplicação
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Acopla os Behaviors na Pipeline de execução do MediatR
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}`
      }
    ]
  },
  {
    name: "Sgae.API",
    type: "folder",
    path: "Sgae.API",
    children: [
      {
        name: "Middlewares",
        type: "folder",
        path: "Sgae.API/Middlewares",
        children: [
          {
            name: "ExceptionHandlingMiddleware.cs",
            type: "file",
            path: "Sgae.API/Middlewares/ExceptionHandlingMiddleware.cs",
            language: "csharp",
            description: "Middleware de exceções global para capturar erros e convertê-los em respostas padronizadas da RFC 7807 (Problem Details).",
            content: `using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sgae.API.Middlewares;

/// <summary>
/// Middleware global para capturar e normalizar erros (RFC 7807) antes de responder ao cliente.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu uma exceção não tratada na requisição no SGAE.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, errors) = exception switch
        {
            // Erro de Domínio - Invariante violada (ex: agendamento retroativo)
            Sgae.Domain.Exceptions.DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                domainEx.Message,
                null
            ),
            // Erro de Validação de dados de entrada do FluentValidation (etapas de entrada)
            FluentValidation.ValidationException valEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation Failed",
                "Um ou mais erros de validação ocorreram na entrada dos dados.",
                ExtractValidationErrors(valEx)
            ),
            // Exceção de Argumentos inválidos
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Invalid Parameter",
                argEx.Message,
                null
            ),
            // Outros erros genéricos inexplicados
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Ocorreu um erro interno inesperado nos servidores do SGAE.",
                null
            )
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors != null)
        {
            problemDetails.Extensions.Add("errors", errors);
        }

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }

    private static Dictionary<string, string[]> ExtractValidationErrors(FluentValidation.ValidationException exception)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var error in exception.Errors)
        {
            if (errors.ContainsKey(error.PropertyName))
            {
                var list = new List<string>(errors[error.PropertyName]) { error.ErrorMessage };
                errors[error.PropertyName] = list.ToArray();
            }
            else
            {
                errors[error.PropertyName] = new[] { error.ErrorMessage };
            }
        }
        return errors;
    }
}`
          }
        ]
      },
      {
        name: "Program.cs",
        type: "file",
        path: "Sgae.API/Program.cs",
        language: "csharp",
        description: "Entry point da Web API .NET 8 que amarra a Clean Architecture, registrando dependências e middlewares.",
        content: `using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Sgae.Application;
using Sgae.Application.Abstractions;
using Sgae.Infrastructure.Persistence;
using Sgae.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Serviços das Camadas de Arquitetura Clean
builder.Services.AddApplication(); // Registra o MediatR e pipeline CQRS via método de extensão da Application

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro das implementações físicas da camada de Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

var app = builder.Build();

// ATIVE O MIDDLEWARE DE EXCEÇÕES GLOBAL (RFC 7807)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();`
      }
    ]
  }
];
