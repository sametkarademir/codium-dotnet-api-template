using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

/// <summary>
/// Interfaces for objects that track modification information.
/// </summary>
public interface IModificationAuditedObject : 
    IHasModificationTime,
    IMayHaveModifier
{
}

/// <summary>
/// Interfaces for objects that track modification information.
/// </summary>
public interface IModificationAuditedObject<TUser> : 
    IModificationAuditedObject,
    IMayHaveModifier<TUser>
    where TUser : IEntity
{
}