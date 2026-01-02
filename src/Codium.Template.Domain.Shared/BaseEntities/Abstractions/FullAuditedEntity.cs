using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

[Serializable]
public abstract class FullAuditedEntity : AuditedEntity, IFullAuditedObject
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }
}

[Serializable]
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAuditedObject
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    protected FullAuditedEntity()
    {
    }

    protected FullAuditedEntity(TKey id) : base(id)
    {
    }
}

[Serializable]
public abstract class FullAuditedEntityWithUser<TKey, TUser> : AuditedEntityWithUser<TKey, TUser>, IFullAuditedObject<TUser> 
    where TUser : IEntity
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    public TUser? Deleter { get; set; }

    protected FullAuditedEntityWithUser()
    {
    }

    protected FullAuditedEntityWithUser(TKey id) : base(id)
    {
    }
}