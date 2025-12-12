using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalConfigurations(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var entityClrType = entityType.ClrType;
            var entityInterfaces = entityClrType.GetInterfaces();

            var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
            if (isIEntity)
            {
                var idProperty = entityClrType.GetProperty("Id");
                if (idProperty != null)
                {
                    builder.Entity(entityClrType)
                        .Property(idProperty.Name)
                        .ValueGeneratedOnAdd();
                }
            }

            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).Property
                        (nameof(ICreationAuditedObject.CreationTime))
                    .IsRequired();

                builder.Entity(entityType.ClrType)
                    .Property(nameof(ICreationAuditedObject.CreatorId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(ICreationAuditedObject.CreatorId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(ICreationAuditedObject.CreationTime));
            }

            if (typeof(ICreationAuditedObject<>).IsAssignableFrom(entityType.ClrType))
            {
                var creationAuditInterface = entityType.ClrType.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ICreationAuditedObject<>)
                    );

                if (creationAuditInterface != null)
                {
                    var userType = creationAuditInterface.GetGenericArguments()[0];

                    builder.Entity(entityType.ClrType)
                        .HasOne(userType, "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired(false);
                }
            }

            if (typeof(IAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModificationTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModifierId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(IAuditedObject.LastModifierId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IAuditedObject.LastModificationTime));
            }

            if (typeof(IAuditedObject<>).IsAssignableFrom(entityType.ClrType))
            {
                var auditInterface = entityType.ClrType.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IAuditedObject<>)
                    );

                if (auditInterface != null)
                {
                    var userType = auditInterface.GetGenericArguments()[0];

                    builder.Entity(entityType.ClrType)
                        .HasOne(userType, "LastModifier")
                        .WithMany()
                        .HasForeignKey("LastModifierId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired(false);
                }
            }

            if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeletionTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeleterId))
                    .HasMaxLength(256)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.DeleterId));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.DeletionTime));
                builder.Entity(entityType.ClrType).HasIndex(nameof(IDeletionAuditedObject.IsDeleted));
            }

            if (typeof(IDeletionAuditedObject<>).IsAssignableFrom(entityType.ClrType))
            {
                var deletionAuditInterface = entityType.ClrType.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDeletionAuditedObject<>)
                    );

                if (deletionAuditInterface != null)
                {
                    var userType = deletionAuditInterface.GetGenericArguments()[0];

                    builder.Entity(entityType.ClrType)
                        .HasOne(userType, "Deleter")
                        .WithMany()
                        .HasForeignKey("DeleterId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired(false);
                }
            }
        }
    }

    public static void ApplyGlobalEntityConfigurations<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        var entityType = typeof(T);
        var entityInterfaces = entityType.GetInterfaces();

        var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
        if (isIEntity)
        {
            var idProperty = entityType.GetProperty("Id");
            if (idProperty != null)
            {
                builder.Property(idProperty.Name)
                    .ValueGeneratedOnAdd();
            }
        }

        if (typeof(ISoftDelete).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false)
                ),
                parameter
            );

            builder.HasQueryFilter((LambdaExpression)filter);
        }

        if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ICreationAuditedObject.CreationTime))
                .IsRequired();

            builder.Property(nameof(ICreationAuditedObject.CreatorId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(ICreationAuditedObject.CreatorId));
            builder.HasIndex(nameof(ICreationAuditedObject.CreationTime));
        }
        
        if (typeof(ICreationAuditedObject<>).IsAssignableFrom(entityType))
        {
            var creationAuditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICreationAuditedObject<>)
                );

            if (creationAuditInterface != null)
            {
                var userType = creationAuditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "Creator")
                    .WithMany()
                    .HasForeignKey("CreatorId")
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            }
        }

        if (typeof(IAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IAuditedObject.LastModificationTime))
                .IsRequired(false);

            builder.Property(nameof(IAuditedObject.LastModifierId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IAuditedObject.LastModifierId));
            builder.HasIndex(nameof(IAuditedObject.LastModificationTime));
        }
        
        if (typeof(IAuditedObject<>).IsAssignableFrom(entityType))
        {
            var auditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IAuditedObject<>)
                );

            if (auditInterface != null)
            {
                var userType = auditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "LastModifier")
                    .WithMany()
                    .HasForeignKey("LastModifierId")
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            }
        }

        if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IDeletionAuditedObject.DeletionTime))
                .IsRequired(false);

            builder.Property(nameof(IDeletionAuditedObject.DeleterId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IDeletionAuditedObject.DeleterId));
            builder.HasIndex(nameof(IDeletionAuditedObject.DeletionTime));
            builder.HasIndex(nameof(IDeletionAuditedObject.IsDeleted));
        }
        
        if (typeof(IDeletionAuditedObject<>).IsAssignableFrom(entityType))
        {
            var deletionAuditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IDeletionAuditedObject<>)
                );

            if (deletionAuditInterface != null)
            {
                var userType = deletionAuditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "Deleter")
                    .WithMany()
                    .HasForeignKey("DeleterId")
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            }
        }
    }
}