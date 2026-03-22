using Microsoft.EntityFrameworkCore;

namespace Asambleas.Common;

/// <summary>
/// Request genérico de paginación. PageSize se limita entre 1 y 100.
/// </summary>
public record PagedRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Math.Max(1, Page) - 1) * Take;
    public int Take => Math.Clamp(PageSize, 1, 100);
}

/// <summary>
/// Response genérico paginado con metadatos.
/// </summary>
public record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>
/// Extension methods para paginación sobre IQueryable.
/// </summary>
public static class QueryableExtensions
{
    public static async Task<PagedResponse<T>> ToPagedAsync<T>(
        this IQueryable<T> query, PagedRequest request, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(ct);
        return new PagedResponse<T>(items, total, request.Page, request.Take);
    }
}
