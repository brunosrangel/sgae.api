using System.Threading;
using System.Threading.Tasks;

namespace Sgae.Application.Abstractions;

/// <summary>
/// Contrato do Unit of Work para persistir modificações de forma transacional e atômica.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}