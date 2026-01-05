using FlowDo.EntityFrameworkCore.Contexts;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace FlowDo.IntegrationTests.Base.Common;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationTestWebAppFactory()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("flowdo_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithPortBinding(5432, true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient();

            // Remove existing DbContext registration
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            var connectionString = _postgreSqlContainer.GetConnectionString();

            // Register DbContext with test database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHangfire((provider, config) =>
            {
                config.UsePostgreSqlStorage(c =>
                {
                    c.UseNpgsqlConnection(connectionString);
                }, new PostgreSqlStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    InvisibilityTimeout = TimeSpan.FromMinutes(5)
                });
            });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
                options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", connectionString);

        using (var scope = Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

