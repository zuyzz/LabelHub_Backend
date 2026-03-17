using System.Linq.Expressions;
using DataLabelProject.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Shared.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResponse<TResult>> ToPagedResponseAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        PaginationParameters @params,
        Func<TSource, TResult> map)
    {
        var totalItems = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return new PagedResponse<TResult>
        {
            Items = items.Select(map).ToList(),
            TotalItems = totalItems,
            Page = @params.Page,
            PageSize = @params.PageSize
        };
    }
}