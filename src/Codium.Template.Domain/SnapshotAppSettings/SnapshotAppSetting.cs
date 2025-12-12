using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.SnapshotLogs;

namespace Codium.Template.Domain.SnapshotAppSettings;

[DisableAuditLog]
public class SnapshotAppSetting : CreationAuditedEntity<Guid>
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;

    public Guid SnapshotLogId { get; set; }
    public SnapshotLog? SnapshotLog { get; set; }
}