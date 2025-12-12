using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Querying;
using Microsoft.EntityFrameworkCore.Query;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IGetListRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<List<TEntity>> GetAllSortedAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<PagedList<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<PagedList<TEntity>> GetListSortedAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<SortRequest>? sorts = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
}