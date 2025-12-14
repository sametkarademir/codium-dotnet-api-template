using Codium.Template.Application.Contracts.CronJobs;
using Codium.Template.Application.Contracts.SnapshotLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application;

public class ApplicationSeedInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationSeedInitializer>>();
        
        logger.LogInformation("Application is starting. Taking snapshot log...");
        
        var snapshotLogInitializerService = scope.ServiceProvider.GetRequiredService<ISnapshotLogAppService>();
        await snapshotLogInitializerService.TakeSnapshotLogAsync();
        
        logger.LogInformation("Snapshot log taken successfully.");
        
        var hangfireSeeder = scope.ServiceProvider.GetRequiredService<IHangfireJobSeederContributor>();
        await hangfireSeeder.SeedAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}