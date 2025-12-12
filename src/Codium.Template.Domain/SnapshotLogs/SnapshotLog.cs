using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.SnapshotAppSettings;
using Codium.Template.Domain.SnapshotAssemblies;

namespace Codium.Template.Domain.SnapshotLogs;

[DisableAuditLog]
public class SnapshotLog : CreationAuditedEntity<Guid>
{
    public string? ApplicationName { get; set; }
    public string? ApplicationVersion { get; set; }
    public string? Environment { get; set; }
    public string? MachineName { get; set; }
    public string? MachineOsVersion { get; set; }
    public string? Platform { get; set; }
    public string? CultureInfo { get; set; }
    
    public string? CpuCoreCount { get; set; }
    public string? CpuArchitecture { get; set; }
    public string? TotalRam { get; set; }
    public string? TotalDiskSpace { get; set; }
    public string? FreeDiskSpace { get; set; }
    public string? IpAddress { get; set; }
    public string? Hostname { get; set; }

    public ICollection<SnapshotAssembly> SnapshotAssemblies { get; set; } = [];
    public ICollection<SnapshotAppSetting> SnapshotAppSettings { get; set; } = [];
}