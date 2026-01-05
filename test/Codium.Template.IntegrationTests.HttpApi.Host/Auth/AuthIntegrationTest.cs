using System.Text;
using System.Text.Json;
using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Auth;

[Collection("Auth Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class AuthIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AuthIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Login_Admin_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "admin@codium.com",
            Password = "Pp123456*"
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

            _testOutputHelper.WriteLine($"[SUCCESS] Admin login successful");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Admin login failed");
        }
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Refresh_Token_Async()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "admin@codium.com",
            Password = "Pp123456*"
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await HttpClient.PostAsync("/api/v1/auth/login", loginContent);
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginToken = JsonSerializer.Deserialize<LoginResponseDto>(loginBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = loginToken!.RefreshToken
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(refreshRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/refresh-token", content);

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

            _testOutputHelper.WriteLine($"[SUCCESS] Token refreshed successfully");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Token refresh failed");
        }
    }
}
