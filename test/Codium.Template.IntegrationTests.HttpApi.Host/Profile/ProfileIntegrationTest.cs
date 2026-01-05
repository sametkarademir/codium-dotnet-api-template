using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Codium.Template.IntegrationTests.HttpApi.Host.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Profile;

[Collection("Feature Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class ProfileIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    // Factory is available from base class
    private const string TestUserEmail = "profiletest@flowdo.com";
    private const string TestUserPassword = "ProfileTest123!";
    private const string NewPassword = "NewProfilePassword123!";
    private static string _testUserAccessToken = string.Empty;

    public ProfileIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
        
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Get_Admin_Profile_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/profile");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ProfileResponseDto>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(profile);
            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.NotEmpty(profile.Email);
            Assert.Equal("admin@codium.com", profile.Email);
            Assert.NotEmpty(profile.FirstName);
            Assert.NotEmpty(profile.LastName);

            _testOutputHelper.WriteLine($"[SUCCESS] Admin profile retrieved");
            _testOutputHelper.WriteLine($"[INFO] Email: {profile.Email}, Name: {profile.FirstName} {profile.LastName}");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Get profile failed");
        }
    }

    [Fact, TestPriority(2)]
    public async Task Failure_Should_Not_Get_Profile_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/v1/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Unauthorized access correctly rejected");
    }

    [Fact, TestPriority(3)]
    public async Task Failure_Should_Not_Get_Profile_With_Invalid_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token_12345");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Invalid token correctly rejected");
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Register_Test_User_For_Password_Change_Async()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "Profile",
            LastName = "Test",
            PhoneNumber = "5554443322",
            Email = TestUserEmail,
            Password = TestUserPassword,
            ConfirmPassword = TestUserPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/register", content);

        // Assert
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
        {
            _testOutputHelper.WriteLine($"[SUCCESS] Test user registered or already exists");
            

            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == TestUserEmail);
            
            if (user != null && !user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await dbContext.SaveChangesAsync();
                _testOutputHelper.WriteLine($"[SUCCESS] Test user email confirmed");
            }
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Registration failed");
        }
    }

    [Fact, TestPriority(5)]
    public async Task Success_Should_Login_Test_User_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = TestUserPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/login", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(tokenResponse);
            Assert.NotEmpty(tokenResponse.AccessToken);
            _testUserAccessToken = tokenResponse.AccessToken;

            _testOutputHelper.WriteLine($"[SUCCESS] Test user logged in successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Login failed");
        }
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Get_Test_User_Profile_Async()
    {
        // Arrange
        Assert.NotEmpty(_testUserAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _testUserAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/profile");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ProfileResponseDto>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(profile);
            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.Equal(TestUserEmail, profile.Email);
            Assert.Equal("Profile", profile.FirstName);
            Assert.Equal("Test", profile.LastName);

            _testOutputHelper.WriteLine($"[SUCCESS] Test user profile retrieved");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Get test user profile failed");
        }
    }

    [Fact, TestPriority(7)]
    public async Task Success_Should_Change_Password_Async()
    {
        // Arrange
        Assert.NotEmpty(_testUserAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _testUserAccessToken);

        var changePasswordRequest = new ChangePasswordUserRequestDto
        {
            OldPassword = TestUserPassword,
            NewPassword = NewPassword,
            ConfirmNewPassword = NewPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/profile/change-password", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _testOutputHelper.WriteLine($"[SUCCESS] Password changed successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Change password failed");
        }
    }

    [Fact, TestPriority(8)]
    public async Task Failure_Should_Not_Login_With_Old_Password_After_Change_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = TestUserPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Login with old password correctly rejected after password change");
    }

    [Fact, TestPriority(9)]
    public async Task Success_Should_Login_With_New_Password_After_Change_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = NewPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/login", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(tokenResponse);
            Assert.NotEmpty(tokenResponse.AccessToken);

            _testOutputHelper.WriteLine($"[SUCCESS] Login with new password successful");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Login with new password failed");
        }
    }

    [Fact, TestPriority(10)]
    public async Task Failure_Should_Not_Change_Password_With_Wrong_Current_Password_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = NewPassword
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await HttpClient.PostAsync("/api/v1/auth/login", loginContent);
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<LoginResponseDto>(loginBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse!.AccessToken);

        var changePasswordRequest = new ChangePasswordUserRequestDto
        {
            OldPassword = "WrongPassword123!",
            NewPassword = "AnotherPassword123!",
            ConfirmNewPassword = "AnotherPassword123!"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/profile/change-password", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Wrong current password correctly rejected");
    }

    [Fact, TestPriority(11)]
    public async Task Failure_Should_Not_Change_Password_With_Mismatched_New_Passwords_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var changePasswordRequest = new ChangePasswordUserRequestDto
        {
            OldPassword = "Pp123456*",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "DifferentPassword123!" // Farklı şifre
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/profile/change-password", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Mismatched new passwords correctly rejected");
    }

    [Fact, TestPriority(12)]
    public async Task Failure_Should_Not_Change_Password_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        var changePasswordRequest = new ChangePasswordUserRequestDto
        {
            OldPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/profile/change-password", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Unauthorized password change correctly rejected");
    }
}

