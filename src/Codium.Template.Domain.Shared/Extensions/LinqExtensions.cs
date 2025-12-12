using System.Linq.Expressions;

namespace Codium.Template.Domain.Shared.Extensions;

/// <summary>
/// Provides extension methods for LINQ queries.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Filters a queryable collection based on a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>
    /// <param name="query">The queryable collection to filter.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="predicate">The predicate to apply to the queryable collection.</param>
    /// <returns>The filtered queryable collection.</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Filters an enumerable collection based on a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable collection.</typeparam>
    /// <param name="source">The enumerable collection to filter.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="predicate">The predicate to apply to the enumerable collection.</param>
    /// <returns>The filtered enumerable collection.</returns>
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}