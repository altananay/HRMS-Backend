using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    internal static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return PagedResult<T>.Empty(page);
            }

            var items = await query
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, page.Page, page.PageSize, totalCount);
        }
    }
}
