namespace Codium.Template.Application.Contracts.Profiles;

public class ProfileResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    public bool TwoFactorEnabled { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? PasswordChangedTime { get; set; }

    public List<string> Roles { get; set; } = [];
    public Guid? SessionId { get; set; }
}