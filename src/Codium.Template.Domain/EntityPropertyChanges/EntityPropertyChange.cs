using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;

namespace Codium.Template.Domain.EntityPropertyChanges;

[DisableAuditLog]
public class EntityPropertyChange : CreationAuditedEntity<Guid>
{
    public string PropertyName { get; set; } = null!;
    public string PropertyTypeFullName { get; set; } = null!;
    public string? NewValue { get; set; }
    public string? OriginalValue { get; set; }

    public Guid AuditLogId { get; set; }
    public AuditLog? AuditLog { get; set; }
}