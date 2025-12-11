using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

public interface IMayHaveDeleter
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who deleted the entity.
    /// </summary>
    Guid? DeleterId { get; set; }
}

public interface IMayHaveDeleter<TUser> : IMayHaveDeleter
    where TUser : IEntity
{
    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    TUser? Deleter { get; set; }
}