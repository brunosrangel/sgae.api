using System;
using System.Collections.Generic;

namespace Sgae.Application.Common.Models;

/// <summary>
/// Estrutura de DTO genérica para padronizar as respostas de paginação na API.
/// </summary>
/// <typeparam name="T">O tipo do objeto contido na lista paginada.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Elementos paginados correspondentes à página atual.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// O número da página atual.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// A quantidade máxima de elementos exibidos por página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Quantidade total de elementos correspondentes ao filtro executado.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Quantidade total de páginas disponíveis.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica se existe uma página anterior disponível para navegação.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se existe uma próxima página disponível para navegação.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
}
