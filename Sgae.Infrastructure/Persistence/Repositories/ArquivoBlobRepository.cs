using Microsoft.EntityFrameworkCore;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de acesso a dados para Arquivos e Imagens hospedados no Vercel Blob.
/// </summary>
public class ArquivoBlobRepository : Repository<ArquivoBlob>, IArquivoBlobRepository
{
    public ArquivoBlobRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ArquivoBlob>> GetByCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
    {
        return await Context.ArquivosBlob
            .Where(a => a.Categoria != null && a.Categoria.ToLower() == categoria.ToLower())
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArquivoBlob>> GetByEntidadeRelacionadaIdAsync(Guid entidadeId, CancellationToken cancellationToken = default)
    {
        return await Context.ArquivosBlob
            .Where(a => a.EntidadeRelacionadaId == entidadeId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
