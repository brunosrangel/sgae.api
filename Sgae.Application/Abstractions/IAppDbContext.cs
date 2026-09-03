using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;

namespace Sgae.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Lead> Leads { get; }
    DbSet<LeadHistorico> LeadsHistoricos { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<PerfilConsulente> PerfisConsulentes { get; }
    DbSet<AtendimentoEspiritual> AtendimentosEspirituais { get; }
    DbSet<CanalCaptacao> CanaisCaptacao { get; }
    DbSet<Sacerdote> Sacerdotes { get; }
    DbSet<ServicoConsulta> ServicosConsulta { get; }
    DbSet<RitualSugerido> RituaisSugeridos { get; }
    DbSet<LocalRealizacao> LocaisRealizacao { get; }
    DbSet<CategoriaInsumo> CategoriasInsumos { get; }
    DbSet<CategoriaDespesa> CategoriasDespesas { get; }
    DbSet<CustoInsumo> CustosInsumos { get; }
    DbSet<Prescricao> Prescricoes { get; }
    DbSet<Conversao> Conversoes { get; }
    DbSet<PastoralRole> PastoralRoles { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}