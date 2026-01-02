namespace Codium.Template.Application.Contracts.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = null!;
    public long ExpiryTime { get; set; }
    public string RefreshToken { get; set; } = null!;
}