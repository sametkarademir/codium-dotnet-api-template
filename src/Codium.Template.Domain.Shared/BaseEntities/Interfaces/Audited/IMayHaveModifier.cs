using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

/// <summary>
/// Interfaces for objects that may have a modifier user.
/// </summary>
public interface IMayHaveModifier
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    Guid? LastModifierId { get; set; }
}

/// <summary>
/// Interfaces for objects that may have a modifier user.
/// </summary>
public interface IMayHaveModifier<TUser> : IMayHaveModifier
    where TUser : IEntity
{
    /// <summary>
    /// Gets or sets the last modifier user.
    /// </summary>
    TUser? LastModifier { get; set; }
}