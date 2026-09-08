using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;

namespace Sgae.Infrastructure.Persistence;

/// <summary>
/// Serviço de semeadura de identidade (IdentitySeeder) encarregado de inicializar
/// os papéis de autorização (Pastoral Roles: Admin, Sacerdote, Secretaria, Consulente)
/// e garantir que o usuário Administrador ('admin@sgae.com.br') e operadores padrão
/// estejam configurados, ativos e sincronizados com as credenciais corretas.
/// </summary>
public class IdentitySeeder : IIdentitySeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public IdentitySeeder(
        AppDbContext context,
        ILogger<IdentitySeeder> logger,
        IPasswordHasher passwordHasher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Executa de forma resiliente e idempotente a semeadura dos papéis (Roles) e dos usuários padrão.
    /// </summary>
    public async Task SeedIdentityAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SGAE IdentitySeeder: Iniciando verificação e sincronização de Roles e Usuários Administrativos...");

        try
        {
            // 1. Garantir os Papéis de Autorização (Roles)
            var roleMap = await SeedRolesAsync(cancellationToken);

            // 2. Garantir o Administrador ('admin@sgae.com.br') e operadores padrão
            await SeedUsersAsync(roleMap, cancellationToken);

            _logger.LogInformation("SGAE IdentitySeeder: Sincronização de Identidade e Acesso finalizada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGAE IdentitySeeder: Falha durante a semeadura de identidade.");
            throw;
        }
    }

    private async Task<Dictionary<string, PastoralRole>> SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existingRoles = await _context.PastoralRoles.ToListAsync(cancellationToken);
        var roleDict = existingRoles.ToDictionary(r => r.Nome, r => r, StringComparer.OrdinalIgnoreCase);

        var requiredRoles = new[]
        {
            new
            {
                Nome = "Admin",
                Descricao = "Administrador de sistemas com privilégios irrestritos de configuração, auditoria e controle de acessos.",
                Escopo = "System.All,Pastoral.All"
            },
            new
            {
                Nome = "Sacerdote",
                Descricao = "Sacerdote / Babalorixá responsável por consultas oraculares, jogos de búzios e rituais sagrados.",
                Escopo = "Pastoral.Atendimento,Pastoral.Acompanhamento,Pastoral.Leitura,Pastoral.Sacerdote"
            },
            new
            {
                Nome = "Secretaria",
                Descricao = "Equipe de acolhimento e secretaria encarregada da gestão de leads, agendamentos e recepção.",
                Escopo = "Pastoral.Agendamento,Pastoral.Lead,Pastoral.Leitura"
            },
            new
            {
                Nome = "Consulente",
                Descricao = "Consulente ou visitante da casa com acesso ao seu próprio histórico de agendamentos e orientações.",
                Escopo = "Pastoral.Consulente,Pastoral.Leitura"
            },
            new
            {
                Nome = "Pastor",
                Descricao = "Membro da equipe pastoral responsável por triagens, aconselhamento fraterno, passes e acompanhamento dos consulentes.",
                Escopo = "Pastoral.Atendimento,Pastoral.Acompanhamento,Pastoral.Leitura"
            },
            new
            {
                Nome = "Coordenador",
                Descricao = "Coordenador do fluxo de acolhimento encarregado do controle de leads de captação, agendamentos e status de salas.",
                Escopo = "Pastoral.Agendamento,Pastoral.Lead,Pastoral.Leitura"
            },
            new
            {
                Nome = "PastoralStaff",
                Descricao = "Auxiliar de apoio pastoral focado na acolhida inicial, suporte logístico e leitura de relatórios de presença.",
                Escopo = "Pastoral.Leitura"
            }
        };

        var addedRoles = 0;
        foreach (var req in requiredRoles)
        {
            if (!roleDict.TryGetValue(req.Nome, out var existingRole))
            {
                var newRole = new PastoralRole(req.Nome, req.Descricao, req.Escopo);
                await _context.PastoralRoles.AddAsync(newRole, cancellationToken);
                roleDict[req.Nome] = newRole;
                addedRoles++;
            }
        }

        if (addedRoles > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE IdentitySeeder: {Count} novos papéis (Roles) foram cadastrados.", addedRoles);
        }

        return roleDict;
    }

    private async Task SeedUsersAsync(Dictionary<string, PastoralRole> roleDict, CancellationToken cancellationToken)
    {
        // Localiza ou valida existência do sacerdote padrão para vínculo
        var sacerdote = await _context.Sacerdotes.FirstOrDefaultAsync(s => s.Nome.Contains("Sidnei"), cancellationToken);

        var adminRole = roleDict.GetValueOrDefault("Admin");
        var sacerdoteRole = roleDict.GetValueOrDefault("Sacerdote") ?? roleDict.GetValueOrDefault("Pastor");
        var secretariaRole = roleDict.GetValueOrDefault("Secretaria") ?? roleDict.GetValueOrDefault("Coordenador");
        var consulenteRole = roleDict.GetValueOrDefault("Consulente");

        var usersToSeed = new[]
        {
            // 1. Administrador Principal (obrigatório)
            new
            {
                Nome = "Administrador SGAE",
                Email = "admin@sgae.com.br",
                SenhaPadrao = "Mudar@123",
                Perfil = PerfilUsuario.Admin,
                SacerdoteId = (Guid?)null,
                RoleId = adminRole?.Id
            },
            new
            {
                Nome = "Administrador SGAE",
                Email = "admin@sgae.com",
                SenhaPadrao = "SgaeAdmin2026!",
                Perfil = PerfilUsuario.Admin,
                SacerdoteId = (Guid?)null,
                RoleId = adminRole?.Id
            },

            // 2. Sacerdote
            new
            {
                Nome = "Babalorixá Sidnei",
                Email = "sacerdote@sgae.com.br",
                SenhaPadrao = "Mudar@123",
                Perfil = PerfilUsuario.Sacerdote,
                SacerdoteId = sacerdote?.Id,
                RoleId = sacerdoteRole?.Id
            },
            new
            {
                Nome = "Babalorixá Sidnei",
                Email = "sacerdote@sgae.com",
                SenhaPadrao = "SgaeSacerdote2026!",
                Perfil = PerfilUsuario.Sacerdote,
                SacerdoteId = sacerdote?.Id,
                RoleId = sacerdoteRole?.Id
            },

            // 3. Secretaria
            new
            {
                Nome = "Secretaria Pastoral",
                Email = "secretaria@sgae.com.br",
                SenhaPadrao = "Mudar@123",
                Perfil = PerfilUsuario.Secretaria,
                SacerdoteId = (Guid?)null,
                RoleId = secretariaRole?.Id
            },
            new
            {
                Nome = "Secretaria Pastoral",
                Email = "secretaria@sgae.com",
                SenhaPadrao = "SgaeSecretaria2026!",
                Perfil = PerfilUsuario.Secretaria,
                SacerdoteId = (Guid?)null,
                RoleId = secretariaRole?.Id
            },

            // 4. Consulente
            new
            {
                Nome = "Consulente Visitante",
                Email = "consulente@sgae.com.br",
                SenhaPadrao = "Mudar@123",
                Perfil = PerfilUsuario.Consulente,
                SacerdoteId = (Guid?)null,
                RoleId = consulenteRole?.Id
            },
            new
            {
                Nome = "Consulente Visitante",
                Email = "consulente@sgae.com",
                SenhaPadrao = "SgaeConsulente2026!",
                Perfil = PerfilUsuario.Consulente,
                SacerdoteId = (Guid?)null,
                RoleId = consulenteRole?.Id
            }
        };

        var changesMade = false;
        foreach (var def in usersToSeed)
        {
            var user = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == def.Email, cancellationToken);

            if (user == null)
            {
                var newUser = new Usuario(
                    def.Nome,
                    def.Email,
                    _passwordHasher.HashPassword(def.SenhaPadrao),
                    def.Perfil,
                    def.SacerdoteId,
                    def.RoleId,
                    primeiroAcesso: false
                );
                await _context.Usuarios.AddAsync(newUser, cancellationToken);
                changesMade = true;
                _logger.LogInformation("SGAE IdentitySeeder: Usuário criado: {Email} (Perfil: {Perfil})", def.Email, def.Perfil);
            }
            else
            {
                // Sincroniza se senha ou status ativo estiverem desalinhados
                var passwordMatches = _passwordHasher.VerifyPassword(def.SenhaPadrao, user.PasswordHash);
                if (!passwordMatches)
                {
                    user.UpdatePassword(_passwordHasher.HashPassword(def.SenhaPadrao));
                    changesMade = true;
                    _logger.LogInformation("SGAE IdentitySeeder: Senha padrão sincronizada para o usuário {Email}", def.Email);
                }

                if (!user.StatusAtivo)
                {
                    user.SetStatus(true);
                    changesMade = true;
                    _logger.LogInformation("SGAE IdentitySeeder: Usuário {Email} reativado com sucesso.", def.Email);
                }

                if (def.RoleId.HasValue && user.PastoralRoleId != def.RoleId)
                {
                    user.UpdateProfile(user.Nome, user.Perfil, user.SacerdoteId, def.RoleId, user.StatusAtivo);
                    changesMade = true;
                }
            }
        }

        if (changesMade)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SGAE IdentitySeeder: Atualizações de usuários salvas no banco de dados.");
        }
        else
        {
            _logger.LogInformation("SGAE IdentitySeeder: Todos os usuários padrão já estão devidamente configurados e atualizados.");
        }
    }
}
