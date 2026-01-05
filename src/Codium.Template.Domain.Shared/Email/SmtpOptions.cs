namespace Codium.Template.Domain.Shared.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}

