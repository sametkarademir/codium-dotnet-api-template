using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

/// <summary>
/// Interfaces for objects that track creation information.
/// Combines creation time and creator interfaces.
/// </summary>
public interface ICreationAuditedObject : 
    IHasCreationTime,
    IMayHaveCreator
{
}

/// <summary>
/// Interfaces for objects that track creation information.
/// Combines creation time and creator interfaces.
/// </summary>
public interface ICreationAuditedObject<TUser> : 
    ICreationAuditedObject,
    IMayHaveCreator<TUser>
    where TUser : IEntity
{
}