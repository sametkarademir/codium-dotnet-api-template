namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

/// <summary>
/// Interfaces for objects that track their deletion time.
/// </summary>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets or sets the deletion time of the entity.
    /// </summary>
    DateTime? DeletionTime { get; set; }
}