namespace Sgae.Application.Common.Models;

/// <summary>
/// DTO base contendo os parâmetros de entrada comuns para requisições paginadas.
/// </summary>
public record PaginationQuery
{
    /// <summary>
    /// O número da página solicitada. O padrão é 1 (primeira página).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// A quantidade máxima de elementos por página. O padrão é 10.
    /// </summary>
    public int PageSize { get; init; } = 10;
}
