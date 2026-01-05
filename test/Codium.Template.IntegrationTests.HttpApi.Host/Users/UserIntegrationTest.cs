using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Users;

[Collection("Feature Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class UserIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private static Guid _testUserId = Guid.Empty;
    private static Guid _testRoleId = Guid.Empty;

    public UserIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Get_All_Users_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(users);
        Assert.NotEmpty(users);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {users.Count} users");
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Get_Paged_Users_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/users/paged?Page=1&PerPage=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<UserResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);
        Assert.True(pagedResult.Meta.TotalCount > 0);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} users (Page {pagedResult.Meta.CurrentPage})");
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Create_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createRequest = new CreateUserRequestDto
        {
            Email = $"testuser_{Guid.NewGuid():N}@flowdo.com",
            Password = "TestUser123!",
            ConfirmPassword = "TestUser123!",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = true
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(createRequest.Email, user.Email);
        Assert.Equal(createRequest.FirstName, user.FirstName);
        Assert.Equal(createRequest.LastName, user.LastName);

        _testUserId = user.Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Created user with ID: {_testUserId}");
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Get_User_By_Id_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/users/{_testUserId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        Assert.Equal(_testUserId, user.Id);
        Assert.NotEmpty(user.Email);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved user: {user.Email}");
    }

    [Fact, TestPriority(5)]
    public async Task Success_Should_Update_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var updateRequest = new UpdateUserRequestDto
        {
            FirstName = "Updated",
            LastName = "User",
            IsActive = true
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PutAsync($"/api/v1/users/{_testUserId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        Assert.Equal(updateRequest.FirstName, user.FirstName);
        Assert.Equal(updateRequest.LastName, user.LastName);

        _testOutputHelper.WriteLine($"[SUCCESS] Updated user: {user.Email}");
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Toggle_Email_Confirmation_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/email-confirmation", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Toggled email confirmation for user {_testUserId}");
    }

    [Fact, TestPriority(7)]
    public async Task Success_Should_Toggle_Phone_Number_Confirmation_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/phone-number-confirmation", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Toggled phone number confirmation for user {_testUserId}");
    }

    [Fact, TestPriority(8)]
    public async Task Success_Should_Toggle_Two_Factor_Enabled_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/two-factor-enabled", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Toggled two factor enabled for user {_testUserId}");
    }

    [Fact, TestPriority(9)]
    public async Task Success_Should_Toggle_Is_Active_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/is-active", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Toggled is active for user {_testUserId}");
    }

    [Fact, TestPriority(10)]
    public async Task Success_Should_Get_Roles_For_User_Assignment_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Get roles
        var response = await HttpClient.GetAsync("/api/v1/roles");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<RoleResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(roles);
        Assert.NotEmpty(roles);

        _testRoleId = roles.First().Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {roles.Count} roles for user assignment");
    }

    [Fact, TestPriority(11)]
    public async Task Success_Should_Assign_Role_To_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/assign/{_testRoleId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Assigned role {_testRoleId} to user {_testUserId}");
    }

    [Fact, TestPriority(12)]
    public async Task Success_Should_Unassign_Role_From_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/unassign/{_testRoleId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Unassigned role {_testRoleId} from user {_testUserId}");
    }

    [Fact, TestPriority(14)]
    public async Task Success_Should_Unassign_Multiple_Roles_From_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Get user to see assigned roles
        var userResponse = await HttpClient.GetAsync($"/api/v1/users/{_testUserId}");
        userResponse.EnsureSuccessStatusCode();
        var userBody = await userResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(userBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        
        if (user.Roles.Count == 0)
        {
            _testOutputHelper.WriteLine($"[SKIP] User has no roles to unassign");
            return;
        }

        var roleIds = user.Roles.Select(r => r.Id).ToList();

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(roleIds),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/unassign", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Unassigned {roleIds.Count} roles from user {_testUserId}");
    }

    [Fact, TestPriority(15)]
    public async Task Success_Should_Lock_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/lock", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Locked user {_testUserId}");
    }

    [Fact, TestPriority(16)]
    public async Task Success_Should_Unlock_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/unlock", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Unlocked user {_testUserId}");
    }

    [Fact, TestPriority(17)]
    public async Task Success_Should_Reset_Password_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var resetPasswordRequest = new ResetPasswordUserRequestDto
        {
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(resetPasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PatchAsync($"/api/v1/users/{_testUserId}/reset-password", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Reset password for user {_testUserId}");
    }

    [Fact, TestPriority(18)]
    public async Task Success_Should_Delete_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/users/{_testUserId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Deleted user {_testUserId}");
    }

    [Fact, TestPriority(19)]
    public async Task Failure_Should_Not_Get_Users_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected request without token");
    }

    [Fact, TestPriority(20)]
    public async Task Failure_Should_Not_Get_Non_Existent_User_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly returned NotFound for non-existent user");
    }

    [Fact, TestPriority(21)]
    public async Task Failure_Should_Not_Create_User_With_Invalid_Data_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createRequest = new CreateUserRequestDto
        {
            Email = "invalid-email", // Invalid email format
            Password = "short", // Too short password
            ConfirmPassword = "different" // Mismatched passwords
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected user creation with invalid data");
    }
}

