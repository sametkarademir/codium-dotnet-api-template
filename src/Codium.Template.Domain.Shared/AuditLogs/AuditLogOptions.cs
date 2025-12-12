using Microsoft.EntityFrameworkCore;

namespace Codium.Template.Domain.Shared.AuditLogs;

public class AuditLogOptions
{
    public const string SectionName = "AuditLog";
    
    public bool Enabled { get; set; } = true;
    public bool LogChangeDetails { get; set; } = true;
    public int MaxValueLength { get; set; } = 5000;
    public string MaskPattern { get; set; } = "***MASKED***";
    public HashSet<string> SensitiveProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Token", "Secret", "ApiKey", "Key", "Credential", "Ssn", "Credit", "Card",
        "SecurityCode", "Pin", "Authorization"
    };
    public Dictionary<Type, HashSet<string>> ExcludedPropertiesByEntityType { get; set; } = new();
    public HashSet<Type> IncludedEntityTypes { get; set; } = new();
    public HashSet<Type> ExcludedEntityTypes { get; set; } = new();

    private List<EntityState> LoggedStates { get; set; } = [
        EntityState.Added,
        EntityState.Modified,
        EntityState.Deleted
    ];

    public bool ShouldLogState(EntityState state)
    {
        return LoggedStates.Contains(state);
    }

    public bool ShouldLogEntity(Type entityType)
    {
        // If present in ExcludedEntityTypes, should not be logged
        if (ExcludedEntityTypes.Contains(entityType))
        {
            return false;
        }

        // If IncludedEntityTypes is empty, all entities are logged, otherwise only the specified ones are logged
        return IncludedEntityTypes.Count == 0 || IncludedEntityTypes.Contains(entityType);
    }

    public bool ShouldLogProperty(Type entityType, string propertyName)
    {
        if (ExcludedPropertiesByEntityType.TryGetValue(entityType, out var excludedProperties))
        {
            return !excludedProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        return true;
    }

    public bool IsSensitiveProperty(string propertyName)
    {
        return SensitiveProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }
}