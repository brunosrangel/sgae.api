using System;
using System.Collections.Generic;

namespace Sgae.Application.Common.Models;

/// <summary>
/// Modelo genérico de resposta paginada (DTO) para os resultados das consultas na API.
/// </summary>
/// <typeparam name="T">Tipo do objeto retornado na coleção.</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Coleção de dados retornados na página atual.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// O número da página recuperada.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// A quantidade máxima de termos por página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// O total de registros encontrados no banco de dados.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// O total de páginas calculadas com base no PageSize.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Determina se há elementos em páginas anteriores.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Determina se há elementos nas próximas páginas.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse() { }

    public PagedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
