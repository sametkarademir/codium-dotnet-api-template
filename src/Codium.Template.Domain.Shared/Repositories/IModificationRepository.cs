using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IModificationRepository<TEntity> where TEntity : class, IEntity
{
    ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );
    
    ValueTask<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}