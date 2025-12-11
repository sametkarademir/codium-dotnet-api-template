namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

/// <summary>
/// Interfaces for objects that track their creation time.
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets or sets the creation time of the entity.
    /// </summary>
    DateTime CreationTime { get; set; }
}