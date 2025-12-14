using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.Domain.AuditLogs;

[DisableAuditLog]
public class AuditLog : CreationAuditedEntityWithUser<Guid, User>
{
    public string EntityId { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public EntityState State { get; set; }
    
    public Guid? SnapshotId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? CorrelationId { get; set; }

    public ICollection<EntityPropertyChange> EntityPropertyChanges { get; set; } = [];
}