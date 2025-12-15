namespace Codium.Template.Domain.Shared.RateLimiting;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    
    public bool Enabled { get; set; } = true;
    public GlobalPolicyOptions GlobalPolicy { get; set; } = new();
    public ApiPolicyOptions ApiPolicy { get; set; } = new();
    public AuthPolicyOptions AuthPolicy { get; set; } = new();
}

public class GlobalPolicyOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}

public class ApiPolicyOptions
{
    public int PermitLimit { get; set; } = 30;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 5;
}

public class AuthPolicyOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 2;
}

