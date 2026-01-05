using System.Runtime.CompilerServices;
using System.Text.Json;
using FlowDo.Application.Contracts.Auth;
using FlowDo.IntegrationTests.Base.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FlowDo.IntegrationTests.Base.Base;

public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly HttpClient HttpClient;
    protected string AdminAccessToken = string.Empty;
    protected string AdminRefreshToken = string.Empty;

    protected readonly IntegrationTestWebAppFactory Factory;
    private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);

    private static readonly ConditionalWeakTable<IntegrationTestWebAppFactory, TokenHolder> FactoryTokens = new();

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001")
        });

        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task InitializeAsync()
    {
        await TokenSemaphore.WaitAsync();
        try
        {
            var tokenHolder = FactoryTokens.GetOrCreateValue(Factory);
            
            if (string.IsNullOrWhiteSpace(tokenHolder.AccessToken))
            {
                await GetAdminTokenAsync();
                tokenHolder.AccessToken = AdminAccessToken;
                tokenHolder.RefreshToken = AdminRefreshToken;
            }
            else
            {
                AdminAccessToken = tokenHolder.AccessToken;
                AdminRefreshToken = tokenHolder.RefreshToken;
            }
        }
        finally
        {
            TokenSemaphore.Release();
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "admin@codium.com",
            Password = "Pp123456*"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Token assignment is already protected by semaphore in InitializeAsync
        AdminAccessToken = tokenResponse?.AccessToken ?? string.Empty;
        AdminRefreshToken = tokenResponse?.RefreshToken ?? string.Empty;
    }
    
    private class TokenHolder
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

