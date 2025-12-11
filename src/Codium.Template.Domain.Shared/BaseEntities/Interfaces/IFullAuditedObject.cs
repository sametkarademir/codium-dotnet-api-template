using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces;

/// <summary>
/// Interfaces for fully audited objects that track creation, modification, and deletion information.
/// Combines all audit interfaces for complete entity tracking.
/// </summary>
public interface IFullAuditedObject : 
    IAuditedObject,
    IDeletionAuditedObject
{
}

/// <summary>
/// Interfaces for fully audited objects that track creation, modification, and deletion information.
/// Combines all audit interfaces for complete entity tracking.
/// </summary>
public interface IFullAuditedObject<TUser> : 
    IAuditedObject<TUser>, 
    IDeletionAuditedObject<TUser> 
    where TUser : IEntity
{
}