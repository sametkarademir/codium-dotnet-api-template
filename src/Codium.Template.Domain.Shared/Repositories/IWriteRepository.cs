using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IWriteRepository<TEntity, in TKey> :
    ICreationRepository<TEntity>,
    IModificationRepository<TEntity>,
    IDeletionRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
}

public interface IWriteRepository<TEntity> :
    ICreationRepository<TEntity>,
    IModificationRepository<TEntity>,
    IDeletionRepository<TEntity>
    where TEntity : class, IEntity
{
}