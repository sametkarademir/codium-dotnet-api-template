using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.SnapshotLogs;

namespace Codium.Template.Domain.SnapshotAssemblies;

[DisableAuditLog]
public class SnapshotAssembly : CreationAuditedEntity<Guid>
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Culture { get; set; }
    public string? PublicKeyToken { get; set; }
    public string? Location { get; set; }

    public Guid SnapshotLogId { get; set; }
    public SnapshotLog? SnapshotLog { get; set; }
}