using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface ICreationRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}