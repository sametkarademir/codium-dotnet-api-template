using System.Net;
using System.Text;
using System.Text.Json;
using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Auth;

[Collection("Auth Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class ForgotPasswordIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    // Factory is available from base class
    private const string TestUserEmail = "forgot@flowdo.com";
    private const string TestUserPassword = "OldPassword123!";
    private const string NewPassword = "NewPassword123!";

    public ForgotPasswordIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
        
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Register_Test_User_Async()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "Forgot",
            LastName = "Password",
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
            _testOutputHelper.WriteLine($"[SUCCESS] User registered or already exists: {TestUserEmail}");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Registration failed");
        }
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Confirm_Email_For_Test_User_Async()
    {
        // Arrange
        var confirmationCode = await GetConfirmationCodeFromDatabaseAsync(TestUserEmail, ConfirmationCodeTypes.EmailConfirmation);
        
        if (string.IsNullOrEmpty(confirmationCode))
        {
            _testOutputHelper.WriteLine($"[INFO] Email already confirmed or no code found");
            return;
        }

        var confirmRequest = new ConfirmEmailRequestDto
        {
            Email = TestUserEmail,
            Code = confirmationCode
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(confirmRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/confirm-email", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"[SUCCESS] Email confirmed successfully");
        }
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Request_Password_Reset_Async()
    {
        // Arrange
        var email = TestUserEmail;

        // Act
        var response = await HttpClient.PostAsync($"/api/v1/auth/forgot-password/{email}", null);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _testOutputHelper.WriteLine($"[SUCCESS] Password reset requested successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Password reset request failed");
        }
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Reset_Password_With_Code_Async()
    {
        // Arrange
        var resetCode = await GetConfirmationCodeFromDatabaseAsync(TestUserEmail, ConfirmationCodeTypes.ResetPassword);
        Assert.NotNull(resetCode);
        Assert.NotEmpty(resetCode);

        _testOutputHelper.WriteLine($"[INFO] Reset code retrieved: {resetCode}");

        var resetRequest = new ResetPasswordRequestDto
        {
            Email = TestUserEmail,
            Code = resetCode,
            NewPassword = NewPassword,
            ConfirmNewPassword = NewPassword
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(resetRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/reset-password", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _testOutputHelper.WriteLine($"[SUCCESS] Password reset successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Password reset failed");
        }
    }

    [Fact, TestPriority(5)]
    public async Task Failure_Should_Not_Login_With_Old_Password_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = TestUserPassword // Eski şifre
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
        _testOutputHelper.WriteLine($"[SUCCESS] Login with old password correctly rejected");
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Login_With_New_Password_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = TestUserEmail,
            Password = NewPassword // Yeni şifre
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
            Assert.NotEmpty(tokenResponse.RefreshToken);

            _testOutputHelper.WriteLine($"[SUCCESS] Login successful with new password");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Login failed with new password");
        }
    }

    [Fact, TestPriority(7)]
    public async Task Failure_Should_Not_Reset_Password_With_Invalid_Code_Async()
    {
        // Arrange
        var resetRequest = new ResetPasswordRequestDto
        {
            Email = TestUserEmail,
            Code = "INVALID_CODE_123456",
            NewPassword = "AnotherPassword123!",
            ConfirmNewPassword = "AnotherPassword123!"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(resetRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/reset-password", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        _testOutputHelper.WriteLine($"[SUCCESS] Invalid reset code correctly rejected");
    }

    [Fact, TestPriority(8)]
    public async Task Failure_Should_Not_Reset_Password_With_Mismatched_Passwords_Async()
    {
        // Arrange - Önce yeni bir reset code al
        await HttpClient.PostAsync($"/api/v1/auth/forgot-password/{TestUserEmail}", null);
        
        var resetCode = await GetConfirmationCodeFromDatabaseAsync(TestUserEmail, ConfirmationCodeTypes.ResetPassword);

        var resetRequest = new ResetPasswordRequestDto
        {
            Email = TestUserEmail,
            Code = resetCode ?? "DUMMY_CODE",
            NewPassword = "Password123!",
            ConfirmNewPassword = "DifferentPassword123!" // Farklı şifre
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(resetRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/reset-password", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Mismatched passwords correctly rejected");
    }

    private async Task<string?> GetConfirmationCodeFromDatabaseAsync(string email, ConfirmationCodeTypes codeType)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return null;
        }

        var confirmationCode = await dbContext.ConfirmationCodes
            .AsNoTracking()
            .Where(cc => cc.UserId == user.Id && cc.Type == codeType && cc.IsUsed == false)
            .OrderByDescending(cc => cc.CreationTime)
            .Select(cc => cc.Code)
            .FirstOrDefaultAsync();

        return confirmationCode;
    }
}

