using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

/// <summary>
/// Interface for objects that track deletion information.
/// Combines deletion time and soft delete interfaces.
/// </summary>
public interface IDeletionAuditedObject : 
    IHasDeletionTime,
    IMayHaveDeleter,
    ISoftDelete
{

}

/// <summary>
/// Interface for objects that track deletion information.
/// Combines deletion time and soft delete interfaces.
/// </summary>
public interface IDeletionAuditedObject<TUser> : 
    IDeletionAuditedObject,
    IMayHaveDeleter<TUser>
    where TUser : IEntity
{

}