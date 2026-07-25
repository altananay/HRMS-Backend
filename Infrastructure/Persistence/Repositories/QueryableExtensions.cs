using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    internal static class QueryableExtensions
    {
        /// <summary>
        /// Materialises one page plus the total count.
        /// </summary>
        /// <remarks>
        /// Two round trips (COUNT then SELECT ... LIMIT/OFFSET) rather than one, which is the right
        /// trade here: the alternative window-function approach repeats the full row payload on
        /// every row. Both queries are AsNoTracking because paged reads never write.
        /// </remarks>
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
