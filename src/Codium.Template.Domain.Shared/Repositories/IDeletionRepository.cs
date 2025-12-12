using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IDeletionRepository<TEntity> where TEntity : class, IEntity
{
    ValueTask<TEntity> DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    ValueTask<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}

public interface IDeletionRepository<TEntity, in TKey> : IDeletionRepository<TEntity> where TEntity : class, IEntity<TKey>
{
    Task<TEntity> DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );
}