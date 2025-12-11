using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

/// <summary>
/// Base class for audited entities.
/// Contains creation, modification time and user information.
/// </summary>
[Serializable]
public abstract class AuditedEntity : CreationAuditedEntity, IAuditedObject
{
    /// <summary>
    /// Gets or sets the last modification time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }
}

/// <summary>
/// Base class for audited entities with a specific key type.
/// Contains creation, modification time and user information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
[Serializable]
public abstract class AuditedEntity<TKey> : CreationAuditedEntity<TKey>, IAuditedObject
{
    /// <summary>
    /// Gets or sets the last modification time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditedEntity{TKey}"/> class.
    /// </summary>
    protected AuditedEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected AuditedEntity(TKey id) : base(id)
    {
    }
}

/// <summary>
/// Base class for audited entities with a specific key type.
/// Contains creation, modification time and user information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
/// <typeparam name="TUser">The type of the user who last modified the entity.</typeparam>
[Serializable]
public abstract class AuditedEntityWithUser<TKey, TUser> : CreationAuditedEntityWithUser<TKey, TUser>, IAuditedObject<TUser> 
    where TUser : IEntity
{
    /// <summary>
    /// Gets or sets the last modification time of the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }
    
    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    public TUser? LastModifier { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditedEntity{TKey}"/> class.
    /// </summary>
    protected AuditedEntityWithUser()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditedEntity{TKey}"/> class with a specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected AuditedEntityWithUser(TKey id) : base(id)
    {
    }
}