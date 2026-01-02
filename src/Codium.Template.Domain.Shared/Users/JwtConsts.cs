namespace Codium.Template.Domain.Shared.Users;

public static class JwtConsts
{
    public const string SigningKey = "50488eb4944b1ede9c7e5db1af4dd5a08521fab2617b075b698f4e923a1adb550d22a1f87a06bc12aa272e48d685921b0aed962c1683a7a116bb834bd1975294";
    public const string Issuer = "http://localhost";
    public const string Audience = "http://localhost";
    public const int AccessTokenLifeHours = 1;
    public const int RefreshTokenLifeHours = 24;
}