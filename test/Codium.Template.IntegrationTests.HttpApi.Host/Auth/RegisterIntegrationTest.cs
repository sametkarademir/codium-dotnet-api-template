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
public class RegisterIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    // Factory is available from base class
    private const string TestUserEmail = "register@flowdo.com";
    private const string TestUserPassword = "TestUser123!";

    public RegisterIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
        
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Register_New_User_Async()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName = "User",
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
        if (response.IsSuccessStatusCode)
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _testOutputHelper.WriteLine($"[SUCCESS] User registered successfully: {TestUserEmail}");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Registration failed");
        }
    }

    [Fact, TestPriority(2)]
    public async Task Failure_Should_Not_Login_Without_Email_Confirmation_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto()
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
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Login correctly blocked - email not confirmed");
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Confirm_Email_With_Code_Async()
    {
        // Arrange
        var confirmationCode = await GetConfirmationCodeFromDatabaseAsync(TestUserEmail);
        Assert.NotNull(confirmationCode);
        Assert.NotEmpty(confirmationCode);

        _testOutputHelper.WriteLine($"[INFO] Confirmation code retrieved: {confirmationCode}");

        var confirmRequest = new
        {
            email = TestUserEmail,
            code = confirmationCode
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
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _testOutputHelper.WriteLine($"[SUCCESS] Email confirmed successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Email confirmation failed");
        }
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Login_After_Email_Confirmation_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto()
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
            Assert.NotEmpty(tokenResponse.RefreshToken);

            _testOutputHelper.WriteLine($"[SUCCESS] Login successful after email confirmation");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Login failed after email confirmation");
        }
    }

    [Fact, TestPriority(5)]
    public async Task Failure_Should_Not_Confirm_Email_With_Invalid_Code_Async()
    {
        // Arrange
        var confirmRequest = new ConfirmEmailRequestDto()
        {
            Email = TestUserEmail,
            Code = "INVALID_CODE_123456"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(confirmRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/confirm-email", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        _testOutputHelper.WriteLine($"[SUCCESS] Invalid confirmation code correctly rejected");
    }

    [Fact, TestPriority(6)]
    public async Task Failure_Should_Not_Register_Duplicate_Email_Async()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto()
        {
            Email = TestUserEmail,
            Password = "AnotherPassword123!",
            ConfirmPassword = "AnotherPassword123!",
            FirstName = "Another",
            LastName = "User",
            PhoneNumber = "+905559876543"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Duplicate email registration correctly rejected");
    }

    private async Task<string?> GetConfirmationCodeFromDatabaseAsync(string email)
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
            .Where(cc => cc.UserId == user.Id && cc.Type == ConfirmationCodeTypes.EmailConfirmation && cc.IsUsed == false)
            .OrderByDescending(cc => cc.CreationTime)
            .Select(cc => cc.Code)
            .FirstOrDefaultAsync();

        return confirmationCode;
    }
}

