using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

/// <summary>
/// Base class for fully audited entities.
/// Contains creation, modification, and deletion audit information.
/// </summary>
[Serializable]
public abstract class FullAuditedEntity : AuditedEntity, IFullAuditedObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who deleted the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    /// <summary>
    /// Gets or sets the deletion time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }
}

/// <summary>
/// Base class for fully audited entities with a specific key type.
/// Contains creation, modification, and deletion audit information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
[Serializable]
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAuditedObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who deleted the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    /// <summary>
    /// Gets or sets the deletion time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TKey}"/> class.
    /// </summary>
    protected FullAuditedEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected FullAuditedEntity(TKey id) : base(id)
    {
    }
}

/// <summary>
/// Base class for fully audited entities with a specific key type.
/// Contains creation, modification, and deletion audit information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
/// <typeparam name="TUser">The type of the user who deleted the entity.</typeparam>
[Serializable]
public abstract class FullAuditedEntityWithUser<TKey, TUser> : AuditedEntityWithUser<TKey, TUser>, IFullAuditedObject<TUser> 
    where TUser : IEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who deleted the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    /// <summary>
    /// Gets or sets the deletion time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }
    
    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    public TUser? Deleter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TKey}"/> class.
    /// </summary>
    protected FullAuditedEntityWithUser()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FullAuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected FullAuditedEntityWithUser(TKey id) : base(id)
    {
    }
}