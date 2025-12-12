using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Shared.Attributes;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.AuditLogs;

public static class DbContextAuditLogExtensions
{
    public static async Task SetAuditLogAsync(this DbContext context, IHttpContextAccessor httpContextAccessor, AuditLogOptions auditLogOptions)
    {
        // Skip processing if audit logging is disabled
        if (!auditLogOptions.Enabled)
        {
            return;
        }

        // Get all entities that have changes in the current context
        var auditEntries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in auditEntries)
        {
            var entityType = entry.Entity.GetType();

            // Check if the entity should be audited based on configuration and attributes
            var hasAttribute = entityType.GetCustomAttributes(typeof(DisableAuditLogAttribute), true).Any();
            var shouldLogEntity = auditLogOptions.ShouldLogEntity(entityType);
            var shouldLogState = auditLogOptions.ShouldLogState(entry.State);

            // Skip if entity is excluded from processing or doesn't meet logging criteria
            if (hasAttribute || !shouldLogEntity || !shouldLogState)
            {
                continue;
            }

            // Create a new audit log entry with basic information
            var auditLog = new AuditLog
            {
                // Set correlation, snapshot, and session IDs from the current HTTP context
                CorrelationId = httpContextAccessor.HttpContext?.GetCorrelationId(),
                SnapshotId = httpContextAccessor.HttpContext?.GetSnapshotId(),
                SessionId = httpContextAccessor.HttpContext?.GetSessionId(),
                // Get entity identifier and name
                EntityId = entry.OriginalValues[entry.Metadata.FindPrimaryKey()!.Properties.First().Name]!.ToString() ?? "Unknown",
                EntityName = entry.Metadata.GetTableName() ?? "Unknown",
                State = entry.State,
                CreationTime = DateTime.UtcNow,
                CreatorId = httpContextAccessor.HttpContext?.User.GetUserId(),
            };

            // Process each property of the entity
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                var propertyInfo = entry.Entity.GetType().GetProperty(propertyName);

                // Skip properties that are excluded from processing or don't meet logging criteria
                if (
                    propertyInfo != null &&
                    !auditLogOptions.ShouldLogProperty(entityType, propertyName)
                )
                {
                    continue;
                }

                if (AuditPropertyNames.Contains(propertyName))
                {
                    continue;
                }

                bool ShouldLogChange(string? oldValue, string? newValue, EntityState state)
                {
                    if (property.GetType().GetCustomAttributes(typeof(DisableAuditLogAttribute), true).Any())
                    {
                        return false;
                    }
           
                    // Log if entity is new or values have changed
                    return state == EntityState.Added || !Equals(oldValue, newValue);
                }

                // Get the old and new values of the property
                var oldValue = property.OriginalValue?.ToString();
                var newValue = property.CurrentValue?.ToString();

                if (ShouldLogChange(oldValue, newValue, auditLog.State))
                {
                    // Handle sensitive data masking
                    if (auditLogOptions.IsSensitiveProperty(propertyName))
                    {
                        oldValue = auditLog.State == EntityState.Added ? null : auditLogOptions.MaskPattern;
                        newValue = auditLogOptions.MaskPattern;
                    }

                    // Truncate values that exceed the maximum length
                    if (oldValue != null && oldValue.Length > auditLogOptions.MaxValueLength)
                    {
                        oldValue = oldValue.Substring(0, auditLogOptions.MaxValueLength) + "... (truncated)";
                    }

                    if (newValue != null && newValue.Length > auditLogOptions.MaxValueLength)
                    {
                        newValue = newValue.Substring(0, auditLogOptions.MaxValueLength) + "... (truncated)";
                    }

                    // Create a new property change record
                    var entityPropertyChange = new EntityPropertyChange
                    {
                        // Only store values if detailed logging is enabled
                        NewValue = auditLogOptions.LogChangeDetails ? newValue : null,
                        OriginalValue = auditLogOptions.LogChangeDetails ? (EntityState.Added == auditLog.State ? null : oldValue) : null,
                        PropertyName = propertyName,
                        PropertyTypeFullName = property.Metadata.ClrType.FullName ?? string.Empty,
                        AuditLogId = auditLog.Id,
                        CreationTime = DateTime.UtcNow,
                        CreatorId = httpContextAccessor.HttpContext?.User.GetUserId()
                    };

                    // Add the property change to the audit log
                    auditLog.EntityPropertyChanges.Add(entityPropertyChange);
                }
            }

            // Save the audit log entry to the database
            await context.Set<AuditLog>().AddAsync(auditLog);
        }
    }
    
    private static readonly HashSet<string> AuditPropertyNames =
    [
        nameof(ICreationAuditedObject.CreatorId),
        nameof(ICreationAuditedObject.CreationTime),

        // Modification
        nameof(IAuditedObject.LastModificationTime),
        nameof(IAuditedObject.LastModifierId),

        // Deletion
        nameof(IDeletionAuditedObject.DeletionTime),
        nameof(IDeletionAuditedObject.DeleterId),
    ];
}