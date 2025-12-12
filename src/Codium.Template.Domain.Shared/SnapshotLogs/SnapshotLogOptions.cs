namespace Codium.Template.Domain.Shared.SnapshotLogs;

public class SnapshotLogOptions
{
    public const string SectionName = "SnapshotLog";
    
    public bool Enabled { get; set; } = true;
    public bool IsSnapshotAppSettingEnabled { get; set; } = false;
    public bool IsSnapshotAssemblyEnabled { get; set; } = false;

    public string MaskPattern { get; set; } = "***MASKED***";
    public List<string> SensitiveProperties { get; set; } =
    [
        "Password",
        "Token",
        "Secret",
        "ApiKey",
        "Key",
        "Credential",
        "Ssn",
        "Credit",
        "Card",
        "SecurityCode",
        "Pin",
        "Authorization"
    ];
}