using Sgae.Domain.Entities;

namespace Sgae.Domain.Repositories;

/// <summary>
/// Contrato de repositório para consulta e persistência dos metadados de arquivos armazenados no Vercel Blob.
/// </summary>
public interface IArquivoBlobRepository : IRepository<ArquivoBlob>
{
    Task<IEnumerable<ArquivoBlob>> GetByCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    Task<IEnumerable<ArquivoBlob>> GetByEntidadeRelacionadaIdAsync(Guid entidadeId, CancellationToken cancellationToken = default);
}
