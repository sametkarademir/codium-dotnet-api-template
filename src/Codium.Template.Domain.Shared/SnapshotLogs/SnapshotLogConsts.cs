namespace Codium.Template.Domain.Shared.SnapshotLogs;

public static class SnapshotLogConsts
{
    public const int ApplicationNameMaxLength = 256;
    public const int ApplicationVersionMaxLength = 64;
    public const int EnvironmentMaxLength = 128;
    public const int MachineNameMaxLength = 256;
    public const int MachineOsVersionMaxLength = 256;
    public const int PlatformMaxLength = 128;
    public const int CultureInfoMaxLength = 64;
    
    public const int CpuCoreCountMaxLength = 32;
    public const int CpuArchitectureMaxLength = 64;
    public const int TotalRamMaxLength = 64;
    public const int TotalDiskSpaceMaxLength = 64;
    public const int FreeDiskSpaceMaxLength = 64;
    public const int IpAddressMaxLength = 2048;
    public const int HostnameMaxLength = 256;
    
    public const bool Enabled = true;
    public const bool IsSnapshotAppSettingEnabled = false;
    public const bool IsSnapshotAssemblyEnabled = false;
    
    public const string MaskPattern = "***MASKED***";
    public static List<string> SensitiveProperties { get; set; } =
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