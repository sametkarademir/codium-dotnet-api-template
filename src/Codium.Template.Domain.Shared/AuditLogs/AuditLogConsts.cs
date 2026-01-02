namespace Codium.Template.Domain.Shared.AuditLogs;

public static class AuditLogConsts
{
    public const int EntityIdMaxLength = 256;
    public const int EntityNameMaxLength = 1024;
    
    public const bool Enabled = true;
    
    public const int ValueMaxLength = 5000;
    public const string MaskPattern = "***MASKED***";

    private static HashSet<string> SensitiveProperties =
    [
        "Password", "Token", "Secret", "ApiKey", "Key", "Credential", "Ssn", "Credit", "Card",
        "SecurityCode", "Pin", "Authorization"
    ];

    public static bool IsSensitiveProperty(string propertyName)
    {
        return SensitiveProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }
}