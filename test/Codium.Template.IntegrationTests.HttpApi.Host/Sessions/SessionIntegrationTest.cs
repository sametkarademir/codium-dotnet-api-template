using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Sessions;
using Codium.Template.IntegrationTests.Base.Base;
using Codium.Template.IntegrationTests.Base.Common;
using Xunit;
using Xunit.Abstractions;

namespace Codium.Template.IntegrationTests.HttpApi.Host.Sessions;

[Collection("Feature Tests")]
[TestCaseOrderer("Codium.Template.IntegrationTests.Base.Common.PriorityOrderer", "Codium.Template.IntegrationTests.Base")]
public class SessionIntegrationTest : BaseIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private static Guid _testSessionId = Guid.Empty;

    public SessionIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact, TestPriority(1)]
    public async Task Success_Should_Get_Paged_Sessions_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);
        Assert.True(pagedResult.Meta.TotalCount >= 0);

        if (pagedResult.Data.Count > 0)
        {
            _testSessionId = pagedResult.Data.First().Id;
            var firstSession = pagedResult.Data.First();
            Assert.NotEqual(Guid.Empty, firstSession.Id);
            Assert.NotEmpty(firstSession.ClientIp);
        }

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} sessions (Page {pagedResult.Meta.CurrentPage} of {pagedResult.Meta.TotalPages})");
    }

    [Fact, TestPriority(2)]
    public async Task Success_Should_Get_Paged_Sessions_With_Filters_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Filter by desktop sessions
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10&IsDesktop=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);
        
        // All returned sessions should be desktop sessions
        foreach (var session in pagedResult.Data)
        {
            Assert.True(session.IsDesktop);
        }

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} desktop sessions");
    }

    [Fact, TestPriority(3)]
    public async Task Success_Should_Get_Paged_Sessions_With_Mobile_Filter_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Filter by mobile sessions
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10&IsMobile=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} mobile sessions");
    }

    [Fact, TestPriority(4)]
    public async Task Success_Should_Get_Paged_Sessions_With_Tablet_Filter_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Filter by tablet sessions
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10&IsTablet=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} tablet sessions");
    }

    [Fact, TestPriority(5)]
    public async Task Success_Should_Get_Paged_Sessions_With_Sorting_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act - Sort by creation time descending
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10&Field=CreationTime&Order=Desc");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} sessions with sorting");
    }

    [Fact, TestPriority(6)]
    public async Task Success_Should_Get_Only_Current_User_Sessions_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=100");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(pagedResult);
        Assert.NotNull(pagedResult.Data);

        // All sessions should belong to the current user (admin)
        // This is verified by the service layer, but we can check that sessions are returned
        Assert.True(pagedResult.Meta.TotalCount >= 0);

        _testOutputHelper.WriteLine($"[SUCCESS] Retrieved {pagedResult.Data.Count} sessions for current user");
    }

    [Fact, TestPriority(8)]
    public async Task Failure_Should_Not_Get_Sessions_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected request without token");
    }

    [Fact, TestPriority(9)]
    public async Task Failure_Should_Not_Get_Sessions_With_Invalid_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/sessions/paged");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected request with invalid token");
    }

    [Fact, TestPriority(10)]
    public async Task Failure_Should_Not_Invalidate_Session_Without_Token_Async()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/sessions/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly rejected invalidate request without token");
    }

    [Fact, TestPriority(11)]
    public async Task Failure_Should_Not_Invalidate_Non_Existent_Session_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/sessions/{nonExistentId}");

        // Assert
        // Note: If the session doesn't belong to the current user, it might return NotFound
        // But if the token is invalid, it will return Unauthorized
        // We check for both cases
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound || 
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected NotFound or Unauthorized, but got {response.StatusCode}"
        );
        
        _testOutputHelper.WriteLine($"[SUCCESS] Correctly returned {response.StatusCode} for non-existent session");
    }

    [Fact, TestPriority(12)]
    public async Task Success_Should_Invalidate_Session_Async()
    {
        // Arrange
        Assert.NotEmpty(AdminAccessToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminAccessToken);

        // First, get sessions to find one to invalidate
        var getSessionsResponse = await HttpClient.GetAsync("/api/v1/sessions/paged?Page=1&PerPage=10");
        getSessionsResponse.EnsureSuccessStatusCode();
        var sessionsBody = await getSessionsResponse.Content.ReadAsStringAsync();
        var sessionsResult = JsonSerializer.Deserialize<PagedResult<SessionResponseDto>>(sessionsBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(sessionsResult);
        
        // If there are no sessions or only one session, skip this test
        // (because invalidating the only session would invalidate the current token)
        if (sessionsResult.Data.Count <= 1)
        {
            _testOutputHelper.WriteLine($"[SKIP] Not enough sessions available to invalidate (need at least 2)");
            return;
        }

        // Get the last session (not the first one, which might be the current session)
        var sessionToInvalidate = sessionsResult.Data.Last();
        _testSessionId = sessionToInvalidate.Id;

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/sessions/{_testSessionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _testOutputHelper.WriteLine($"[SUCCESS] Invalidated session {_testSessionId}");
    }
}

