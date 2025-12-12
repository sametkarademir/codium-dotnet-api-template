using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IReadRepository<TEntity, in TKey> :
    IGetRepository<TEntity, TKey>,
    IGetListRepository<TEntity>,
    ICountRepository<TEntity>
    where TEntity : class, IEntity<TKey>
{
}

public interface IReadRepository<TEntity> :
    IGetRepository<TEntity>,
    IGetListRepository<TEntity>,
    ICountRepository<TEntity>
    where TEntity : class, IEntity
{
}