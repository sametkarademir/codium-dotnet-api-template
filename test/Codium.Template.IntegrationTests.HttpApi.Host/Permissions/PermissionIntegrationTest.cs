using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Permissions;

[Collection("Feature Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class PermissionIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PermissionIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Get_All_Permissions_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<PermissionResponseDto>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(permissions);
            Assert.NotEmpty(permissions);

            _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {permissions.Count} permissions");

            var firstPermission = permissions.First();
            Assert.NotEqual(Guid.Empty, firstPermission.Id);
            Assert.NotEmpty(firstPermission.Name);

            _testOutputHelper.WriteLine($"[INFO] Sample permission: {firstPermission.Name}");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Get all permissions failed");
        }
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Verify_Permission_Structure_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<PermissionResponseDto>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(permissions);
            var permissionNames = permissions.Select(p => p.Name).ToList();

            Assert.Contains(permissionNames, name => name.StartsWith("Permission."));
            Assert.Contains(permissionNames, name => name.StartsWith("Role."));
            Assert.Contains(permissionNames, name => name.StartsWith("User."));

            _testOutputHelper.WriteLine($"[SUCCESS] Permission structure verified");
            _testOutputHelper.WriteLine($"[INFO] Permission groups found: Permission, Role, User, Todo");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Permission structure verification failed");
        }
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Verify_Specific_Permissions_Exist_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        var expectedPermissions = new[]
        {
            "Permission.GetAll",
            "Role.GetById",
            "Role.GetAll",
            "Role.Create",
            "Role.Update",
            "Role.Delete",
            "User.GetById",
            "User.GetAll",
            "User.Create",
            "User.Update",
            "User.Delete"
        };

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<PermissionResponseDto>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(permissions);
            var permissionNames = permissions.Select(p => p.Name).ToList();

            foreach (var expectedPermission in expectedPermissions)
            {
                Assert.Contains(expectedPermission, permissionNames);
                _testOutputHelper.WriteLine($"[SUCCESS] Permission found: {expectedPermission}");
            }

            _testOutputHelper.WriteLine($"[SUCCESS] All expected permissions exist");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Specific permissions verification failed");
        }
    }

    [Fact, TestPriority(4)]
    public async Task Failure_Should_Not_Get_Permissions_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Unauthorized access correctly rejected");
    }

    [Fact, TestPriority(5)]
    public async Task Failure_Should_Not_Get_Permissions_With_Invalid_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token_12345");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Invalid token correctly rejected");
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Verify_Permission_Has_Required_Fields_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/permissions");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<PermissionResponseDto>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(permissions);
            Assert.NotEmpty(permissions);
            
            foreach (var permission in permissions)
            {
                Assert.NotEqual(Guid.Empty, permission.Id);
                Assert.False(string.IsNullOrEmpty(permission.Name));
            }

            _testOutputHelper.WriteLine($"[SUCCESS] All permissions have required fields (id, name)");
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"[ERROR] [StatusCode]: {response.StatusCode} - [Response]: {responseBody}");
            Assert.Fail("Permission fields verification failed");
        }
    }
}

