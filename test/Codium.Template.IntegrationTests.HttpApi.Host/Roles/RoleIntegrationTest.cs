using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Roles;

[Collection("Feature Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class RoleIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private static Guid _testRoleId = Guid.Empty;
    private static Guid _testPermissionId = Guid.Empty;
    private static Guid _testUserId = Guid.Empty;

    public RoleIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Get_All_Permissions_For_Role_Tests_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var permissions = JsonSerializer.Deserialize<List<PermissionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);

        _testPermissionId = permissions.First().Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {permissions.Count} permissions for role tests");
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Get_All_Roles_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
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

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {roles.Count} roles");

        var firstRole = roles.First();
        Assert.NotEqual(Guid.Empty, firstRole.Id);
        Assert.NotEmpty(firstRole.Name);
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Get_Paged_Roles_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/roles/paged?Page=1&PageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<RoleResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotEmpty(pagedResult.Data);
        Assert.True(pagedResult.Meta.TotalCount > 0);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} roles (Page {pagedResult.Meta.CurrentPage} of {pagedResult.Meta.TotalPages})");
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Create_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createRequest = new CreateRoleRequestDto
        {
            Name = $"TestRole_{Guid.NewGuid():N}",
            Description = "Test role for integration tests"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/roles", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(role);
        Assert.NotEqual(Guid.Empty, role.Id);
        Assert.Equal(createRequest.Name, role.Name);
        Assert.Equal(createRequest.Description, role.Description);

        _testRoleId = role.Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Created role with ID: {_testRoleId}");
    }

    [Fact, TestPriority(5)]
    public async Task Success_Should_Get_Role_By_Id_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/roles/{_testRoleId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(role);
        Assert.Equal(_testRoleId, role.Id);
        Assert.NotEmpty(role.Name);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved role: {role.Name}");
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Update_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var updateRequest = new UpdateRoleRequestDto
        {
            Name = $"UpdatedTestRole_{Guid.NewGuid():N}",
            Description = "Updated test role description"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PutAsync($"/api/v1/roles/{_testRoleId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(role);
        Assert.Equal(_testRoleId, role.Id);
        Assert.Equal(updateRequest.Name, role.Name);
        Assert.Equal(updateRequest.Description, role.Description);

        _testOutputHelper.WriteLine($"[SUCCESS] Updated role: {role.Name}");
    }

    [Fact, TestPriority(7)]
    public async Task Success_Should_Assign_Permission_To_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        Assert.NotEqual(Guid.Empty, _testPermissionId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/roles/{_testRoleId}/assign/{_testPermissionId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Assigned permission {_testPermissionId} to role {_testRoleId}");
    }

    [Fact, TestPriority(8)]
    public async Task Success_Should_Get_Role_With_Permissions_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/roles/{_testRoleId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(role);
        Assert.NotNull(role.Permissions);
        Assert.Contains(role.Permissions, p => p.Id == _testPermissionId);

        _testOutputHelper.WriteLine($"[SUCCESS] Role has {role.Permissions.Count} permission(s)");
    }

    [Fact, TestPriority(9)]
    public async Task Success_Should_Unassign_Permission_From_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        Assert.NotEqual(Guid.Empty, _testPermissionId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.PatchAsync($"/api/v1/roles/{_testRoleId}/unassign/{_testPermissionId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Unassigned permission {_testPermissionId} from role {_testRoleId}");
    }

    [Fact, TestPriority(12)]
    public async Task Success_Should_Delete_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/roles/{_testRoleId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Deleted role {_testRoleId}");
    }

    [Fact, TestPriority(13)]
    public async Task Failure_Should_Not_Get_Role_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/v1/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected request without token");
    }

    [Fact, TestPriority(14)]
    public async Task Failure_Should_Not_Get_Role_With_Invalid_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected request with invalid token");
    }

    [Fact, TestPriority(15)]
    public async Task Failure_Should_Not_Get_Non_Existent_Role_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/roles/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly returned NotFound for non-existent role");
    }

    [Fact, TestPriority(16)]
    public async Task Failure_Should_Not_Create_Role_With_Invalid_Data_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createRequest = new CreateRoleRequestDto
        {
            Name = string.Empty, // Invalid: empty name
            Description = "Test description"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/roles", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected role creation with invalid data");
    }

    [Fact, TestPriority(17)]
    public async Task Success_Should_Create_User_For_Role_Deletion_Test_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createUserRequest = new CreateUserRequestDto
        {
            Email = $"roletestuser_{Guid.NewGuid():N}@flowdo.com",
            Password = "TestUser123!",
            ConfirmPassword = "TestUser123!",
            FirstName = "Role",
            LastName = "Test",
            IsActive = true,
            EmailConfirmed = true
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createUserRequest),
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

        _testUserId = user.Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Created test user with ID: {_testUserId}");
    }

    [Fact, TestPriority(18)]
    public async Task Success_Should_Create_Role_For_Deletion_Test_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var createRequest = new CreateRoleRequestDto
        {
            Name = $"TestRoleForDeletion_{Guid.NewGuid():N}",
            Description = "Test role for deletion test"
        };

        // Act
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/roles", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(role);
        Assert.NotEqual(Guid.Empty, role.Id);

        _testRoleId = role.Id;
        _testOutputHelper.WriteLine($"[SUCCESS] Created test role with ID: {_testRoleId}");
    }

    [Fact, TestPriority(19)]
    public async Task Success_Should_Assign_Role_To_User_For_Deletion_Test_Async()
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

        // Verify role is assigned
        var getUserResponse = await HttpClient.GetAsync($"/api/v1/users/{_testUserId}");
        getUserResponse.EnsureSuccessStatusCode();
        var userBody = await getUserResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(userBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        Assert.NotNull(user.Roles);
        Assert.Contains(user.Roles, r => r.Id == _testRoleId);

        _testOutputHelper.WriteLine($"[SUCCESS] Assigned role {_testRoleId} to user {_testUserId}");
    }

    [Fact, TestPriority(20)]
    public async Task Success_Should_Delete_Role_And_Verify_User_Roles_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        Assert.NotEqual(Guid.Empty, _testRoleId);
        Assert.NotEqual(Guid.Empty, _testUserId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Delete the role (soft delete)
        var deleteResponse = await HttpClient.DeleteAsync($"/api/v1/roles/{_testRoleId}");

        // Assert - Role deletion should succeed
        deleteResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify role is deleted (should return NotFound when trying to get it)
        var getRoleResponse = await HttpClient.GetAsync($"/api/v1/roles/{_testRoleId}");
        Assert.Equal(HttpStatusCode.NotFound, getRoleResponse.StatusCode);

        // Verify user still exists and check if role appears in user's roles
        var getUserResponse = await HttpClient.GetAsync($"/api/v1/users/{_testUserId}");
        getUserResponse.EnsureSuccessStatusCode();
        var userBody = await getUserResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(userBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(user);
        Assert.NotNull(user.Roles);

        // Check if deleted role still appears in user's roles
        var deletedRoleInUser = user.Roles.FirstOrDefault(r => r.Id == _testRoleId);
        
        if (deletedRoleInUser != null)
        {
            _testOutputHelper.WriteLine($"[INFO] Deleted role {_testRoleId} still appears in user {_testUserId} roles list");
            _testOutputHelper.WriteLine($"[INFO] This indicates that soft-deleted roles are still included in user role queries");
        }
        else
        {
            _testOutputHelper.WriteLine($"[INFO] Deleted role {_testRoleId} does NOT appear in user {_testUserId} roles list");
            _testOutputHelper.WriteLine($"[INFO] This indicates that soft-deleted roles are filtered out from user role queries");
        }

        _testOutputHelper.WriteLine($"[SUCCESS] Role deletion test completed. User has {user.Roles.Count} role(s)");
    }
}

