using Codium.Template.Domain;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "Users", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.NormalizedEmail)
            .IsUnique()
            .HasFilter($"\"{nameof(User.IsDeleted)}\" = FALSE");

        builder.Property(item => item.Email).HasMaxLength(256).IsRequired();
        builder.Property(item => item.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(item => item.EmailConfirmed).HasDefaultValue(false).IsRequired();
        
        builder.Property(item => item.PasswordHash).HasMaxLength(2048).IsRequired();
        
        builder.Property(item => item.PhoneNumber).HasMaxLength(32).IsRequired(false);
        builder.Property(item => item.PhoneNumberConfirmed).HasDefaultValue(false).IsRequired();

        builder.Property(item => item.LockoutEnd).IsRequired(false);
        builder.Property(item => item.LockoutEnabled).HasDefaultValue(true).IsRequired();
        builder.Property(item => item.AccessFailedCount).HasDefaultValue(0).IsRequired();
        
        builder.Property(item => item.FirstName).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.LastName).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.PasswordChangedTime).IsRequired(false);
        builder.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
    }
}