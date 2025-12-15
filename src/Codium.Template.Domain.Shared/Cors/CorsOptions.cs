namespace Codium.Template.Domain.Shared.Cors;

public class CorsOptions
{
    public const string SectionName = "Cors";
    
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowCredentials { get; set; } = true;
}

