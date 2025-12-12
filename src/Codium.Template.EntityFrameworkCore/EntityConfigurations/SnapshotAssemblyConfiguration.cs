using Codium.Template.Domain;
using Codium.Template.Domain.SnapshotAssemblies;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class SnapshotAssemblyConfiguration : IEntityTypeConfiguration<SnapshotAssembly>
{
    public void Configure(EntityTypeBuilder<SnapshotAssembly> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "SnapshotAssemblies", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.Name);

        builder.Property(item => item.Name).HasMaxLength(1024).IsRequired(false);
        builder.Property(item => item.Version).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.Culture).HasMaxLength(64).IsRequired(false);
        builder.Property(item => item.PublicKeyToken).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.Location).HasMaxLength(2048).IsRequired(false);

        builder.HasOne(item => item.SnapshotLog)
            .WithMany(item => item.SnapshotAssemblies)
            .HasForeignKey(item => item.SnapshotLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}