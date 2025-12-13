using Codium.Template.Domain;
using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class ConfirmationCodeConfiguration : IEntityTypeConfiguration<ConfirmationCode>
{
    public void Configure(EntityTypeBuilder<ConfirmationCode> builder)
    {
        builder.ApplyGlobalEntityConfigurations();
        builder.ToTable(ApplicationConsts.DbTablePrefix + "ConfirmationCodes", ApplicationConsts.DbSchema);
        builder.HasIndex(item => new { item.UserId, item.Code })
            .IsUnique()
            .HasFilter($"\"{nameof(ConfirmationCode.IsDeleted)}\" = FALSE");
        builder.HasIndex(item => new { item.UserId, item.Code, item.ExpiryTime, item.IsUsed });

        builder.Property(item => item.Code).HasMaxLength(16).IsRequired();
        builder.Property(item => item.Type).IsRequired();
        builder.Property(item => item.ExpiryTime).IsRequired();
        builder.Property(item => item.IsUsed).IsRequired();
        builder.Property(item => item.UsedAt).IsRequired(false);

        builder.HasOne(item => item.User)
            .WithMany(item => item.ConfirmationCodes)
            .HasForeignKey(item => item.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}