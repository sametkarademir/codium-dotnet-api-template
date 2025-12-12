using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides extension methods for DbContext to handle aggregate root operations
/// </summary>
public static class DbContextAggregateRootExtensions
{
    /// <summary>
    /// Sets creation timestamps for entities that implement ICreationAuditedObject
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="httpContextAccessor">The HTTP context accessor to get user information</param>
    public static void SetCreationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e is
            {
                Entity: ICreationAuditedObject,
                State: EntityState.Added
            });

        foreach (var entry in entries)
        {
            var entity = (ICreationAuditedObject)entry.Entity;
            entity.CreationTime = DateTime.UtcNow;
            entity.CreatorId = httpContextAccessor.HttpContext?.User.GetUserId();
        }
    }

    /// <summary>
    /// Sets modification timestamps for entities that implement IAuditedObject or IHasConcurrencyStamp
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="httpContextAccessor">The HTTP context accessor to get user information</param>
    public static void SetModificationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e is
            {
                Entity: IAuditedObject,
                State: EntityState.Modified
            });

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditedObject)
            {
                var entity = (IAuditedObject)entry.Entity;
                entity.LastModificationTime = DateTime.UtcNow.ToUniversalTime();
                entity.LastModifierId = httpContextAccessor.HttpContext?.User.GetUserId();
            }
        }
    }

    /// <summary>
    /// Sets soft delete timestamps for entities that implement IDeletionAuditedObject
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="httpContextAccessor">The HTTP context accessor to get user information</param>
    public static void SetSoftDelete(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e is { Entity: IDeletionAuditedObject, State: EntityState.Modified } &&
                e.CurrentValues["IsDeleted"]!.Equals(true)
            );

        foreach (var entry in entries)
        {
            var entity = (IDeletionAuditedObject)entry.Entity;
            entity.DeletionTime = DateTime.UtcNow.ToUniversalTime();
            entity.DeleterId = httpContextAccessor.HttpContext?.User.GetUserId();
        }
    }
}