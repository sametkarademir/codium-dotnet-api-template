using Codium.Template.Domain;
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

        builder.Property(item => item.ApplicationName).HasMaxLength(1024).IsRequired(false);
        builder.Property(item => item.ApplicationVersion).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.Environment).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.MachineName).HasMaxLength(256).IsRequired(false);
        builder.Property(item => item.MachineOsVersion).HasMaxLength(256).IsRequired(false);
        builder.Property(item => item.Platform).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.CultureInfo).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.CpuCoreCount).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.CpuArchitecture).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.TotalRam).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.TotalDiskSpace).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.FreeDiskSpace).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.IpAddress).HasMaxLength(2048).IsRequired(false);
        builder.Property(item => item.Hostname).HasMaxLength(256).IsRequired(false);
    }
}