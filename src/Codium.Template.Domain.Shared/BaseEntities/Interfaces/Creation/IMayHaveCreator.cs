using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

/// <summary>
/// Interfaces for objects that may have a creator user.
/// </summary>
public interface IMayHaveCreator
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who created the entity.
    /// </summary>
    Guid? CreatorId { get; set; }
}

/// <summary>
/// Interfaces for objects that may have a creator user.
/// </summary>
public interface IMayHaveCreator<TUser> where TUser : IEntity
{
    /// <summary>
    /// Gets or sets the creator user.
    /// </summary>
    TUser? Creator { get; set; }
}