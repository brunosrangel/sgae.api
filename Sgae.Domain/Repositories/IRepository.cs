using Sgae.Domain.Common;

namespace Sgae.Domain.Repositories;

/// <summary>
/// Contrato de repositório genérico para persistência de dados no projeto Domain, seguindo o padrão Clean Architecture.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}
