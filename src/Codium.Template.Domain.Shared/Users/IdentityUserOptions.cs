namespace Codium.Template.Domain.Shared.Users;

public class IdentityUserOptions
{
    public const string SectionName = "IdentityUser";

    public UserLockoutOptions Lockout { get; set; } = new();
    public PasswordOptions Password { get; set; } = new();

    public JwtOptions Jwt { get; set; } = new();
    public SignInOptions SignIn { get; set; } = new();
}

public class UserLockoutOptions
{
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public TimeSpan DefaultLockoutTimeSpanMinutes { get; set; } = TimeSpan.FromMinutes(15);
    public bool AllowedForNewUsers { get; set; } = true;
}

public class PasswordOptions
{
    public int RequiredLength { get; set; } = 8;
    public int MaxLength { get; set; } = 32;
    public int RequiredUniqueChars { get; set; } = 1;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
}


public class JwtOptions
{
    public string SigningKey { get; set; } = "50488eb4944b1ede9c7e5db1af4dd5a08521fab2617b075b698f4e923a1adb550d22a1f87a06bc12aa272e48d685921b0aed962c1683a7a116bb834bd1975294";
    public string Issuer { get; set; } = "http://localhost";
    public string Audience { get; set; } = "http://localhost";
    public int TokenLifeHours { get; set; } = 1;
    public int RefreshTokenLifeHours { get; set; } = 24;
}

public class SignInOptions
{
    public int MaxActiveSessionsPerUser { get; set; } = 5;
    public bool RequireConfirmedEmail { get; set; } = false;
    public bool RequireConfirmedPhoneNumber { get; set; } = false;
    public int EmailConfirmationCodeExpiryMinutes { get; set; } = 5;
    public int ResetPasswordCodeExpiryMinutes { get; set; } = 5;
}