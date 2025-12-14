using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Querying;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.Domain.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        IEnumerable<SortRequest>? sorts,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        if (sorts == null)
        {
            return queryable;
        }

        var sortRequests = sorts.ToList();
        if (sortRequests.Count == 0)
        {
            return queryable;
        }

        sortRequests = sortRequests.Where(item => !string.IsNullOrWhiteSpace(item.Field)).ToList();
        if (sortRequests.Count == 0)
        {
            return queryable;
        }

        var sortString = string.Join(",", sortRequests.Select(item => $"{item.Field} {item.Order}"));
        
        return queryable.OrderBy(sortString, cancellationToken);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        string? field = null,
        SortOrderTypes orderType = SortOrderTypes.Desc,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        return string.IsNullOrWhiteSpace(field)
            ? queryable
            : queryable.OrderBy($"{field} {orderType.ToString()}", cancellationToken);
    }

    public static async Task<PagedList<T>> ToPageableAsync<T>(
        this IQueryable<T> queryable,
        int page,
        int perPage,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        var count = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        if (count == 0)
        {
            return new PagedList<T>([], 0, page, perPage);
        }

        var items = await queryable
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedList<T>(items, count, page, perPage);
    }
}