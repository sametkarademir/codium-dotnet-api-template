using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

/// <summary>
/// Base class for creation audited entities.
/// Contains creation time and creator user information.
/// </summary>
[Serializable]
public abstract class CreationAuditedEntity : Entity, ICreationAuditedObject
{
    /// <summary>
    /// Gets or sets the creation time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who created the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }
}

/// <summary>
/// Base class for creation audited entities with a specific key type.
/// Contains creation time and creator user information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
[Serializable]
public abstract class CreationAuditedEntity<TKey> : Entity<TKey>, ICreationAuditedObject
{
    /// <summary>
    /// Gets or sets the creation time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who created the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TKey}"/> class.
    /// </summary>
    protected CreationAuditedEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected CreationAuditedEntity(TKey id) : base(id)
    {
    }
}

/// <summary>
/// Base class for creation audited entities with a specific key type.
/// Contains creation time and creator user information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
/// <typeparam name="TUser">The type of the user who created the entity.</typeparam>
[Serializable]
public abstract class CreationAuditedEntityWithUser<TKey, TUser> : Entity<TKey>, ICreationAuditedObject<TUser> 
    where TUser : IEntity
{
    /// <summary>
    /// Gets or sets the creation time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who created the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }
    
    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public TUser? Creator { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TKey}"/> class.
    /// </summary>
    protected CreationAuditedEntityWithUser()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationAuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected CreationAuditedEntityWithUser(TKey id) : base(id)
    {
    }
}