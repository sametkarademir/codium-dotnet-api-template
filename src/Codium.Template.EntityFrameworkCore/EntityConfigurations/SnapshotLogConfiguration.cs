using Codium.Template.Domain;
using Codium.Template.Domain.Shared.SnapshotLogs;
using Codium.Template.Domain.SnapshotLogs;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class SnapshotLogConfiguration : IEntityTypeConfiguration<SnapshotLog>
{
    public void Configure(EntityTypeBuilder<SnapshotLog> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "SnapshotLogs", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.ApplicationName);
        builder.HasIndex(item => item.ApplicationVersion);
        builder.HasIndex(item => item.Environment);
        builder.HasIndex(item => item.MachineName);
        builder.HasIndex(item => item.Platform);
        builder.HasIndex(item => item.IpAddress);
        builder.HasIndex(item => item.Hostname);

        builder.Property(item => item.ApplicationName).HasMaxLength(SnapshotLogConsts.ApplicationNameMaxLength).IsRequired(false);
        builder.Property(item => item.ApplicationVersion).HasMaxLength(SnapshotLogConsts.ApplicationVersionMaxLength).IsRequired(false);
        builder.Property(item => item.Environment).HasMaxLength(SnapshotLogConsts.EnvironmentMaxLength).IsRequired(false);
        builder.Property(item => item.MachineName).HasMaxLength(SnapshotLogConsts.MachineNameMaxLength).IsRequired(false);
        builder.Property(item => item.MachineOsVersion).HasMaxLength(SnapshotLogConsts.MachineOsVersionMaxLength).IsRequired(false);
        builder.Property(item => item.Platform).HasMaxLength(SnapshotLogConsts.PlatformMaxLength).IsRequired(false);
        builder.Property(item => item.CultureInfo).HasMaxLength(SnapshotLogConsts.CultureInfoMaxLength).IsRequired(false);
        builder.Property(item => item.CpuCoreCount).HasMaxLength(SnapshotLogConsts.CpuCoreCountMaxLength).IsRequired(false);
        builder.Property(item => item.CpuArchitecture).HasMaxLength(SnapshotLogConsts.CpuArchitectureMaxLength).IsRequired(false);
        builder.Property(item => item.TotalRam).HasMaxLength(SnapshotLogConsts.TotalRamMaxLength).IsRequired(false);
        builder.Property(item => item.TotalDiskSpace).HasMaxLength(SnapshotLogConsts.TotalDiskSpaceMaxLength).IsRequired(false);
        builder.Property(item => item.FreeDiskSpace).HasMaxLength(SnapshotLogConsts.FreeDiskSpaceMaxLength).IsRequired(false);
        builder.Property(item => item.IpAddress).HasMaxLength(SnapshotLogConsts.IpAddressMaxLength).IsRequired(false);
        builder.Property(item => item.Hostname).HasMaxLength(SnapshotLogConsts.HostnameMaxLength).IsRequired(false);
    }
}