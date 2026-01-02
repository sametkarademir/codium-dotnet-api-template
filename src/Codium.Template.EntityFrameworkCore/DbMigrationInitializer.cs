using Codium.Template.Domain;
using Codium.Template.EntityFrameworkCore.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Codium.Template.EntityFrameworkCore;

public class DbMigrationInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbMigrationInitializer>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Migrating database migrations...");
        
        var canConnect = await context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            logger.LogInformation("Database migrations could not be migrated.");
            
            await context.Database.MigrateAsync(cancellationToken);
            
            logger.LogInformation("Database migrations migrated successfully.");
        }
        else
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Database migrations could not be migrated.");
                
                await context.Database.MigrateAsync(cancellationToken);
                
                logger.LogInformation("Database migrations migrated successfully.");
            }
        }
        
        logger.LogInformation("Database migrations succeeded.");
        
        var identitySeedService = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeederContributor>();
        await identitySeedService.SeedAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}