using Microsoft.EntityFrameworkCore;

namespace Sgae.Application.Common.Models;

/// <summary>
/// Provedor helper de paginação reutilizável para consultas baseadas em IQueryable.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Pagina uma origem IQueryable de forma assíncrona, projetando os resultados para PagedResult.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Safe guards para garantir consistência matemática da paginação
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var count = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, count, pageNumber, pageSize);
    }
}
