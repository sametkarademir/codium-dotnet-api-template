using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

/// <summary>
/// Interfaces for audited objects that track creation and modification information.
/// Combines creation and modification audit interfaces.
/// </summary>
public interface IAuditedObject :
    ICreationAuditedObject,
    IModificationAuditedObject
{
}

/// <summary>
/// Interfaces for audited objects that track creation and modification information.
/// Combines creation and modification audit interfaces.
/// </summary>
public interface IAuditedObject<TUser> : 
    ICreationAuditedObject<TUser>, 
    IModificationAuditedObject<TUser>
    where TUser : IEntity
{
}