using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IQueryableRepository<out TEntity> where TEntity : class, IEntity
{
    IQueryable<TEntity> AsQueryable();
}