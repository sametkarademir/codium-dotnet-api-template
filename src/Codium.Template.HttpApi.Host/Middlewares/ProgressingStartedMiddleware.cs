using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.SnapshotLogs;
using Microsoft.Extensions.Caching.Memory;

namespace Codium.Template.HttpApi.Host.Middlewares;

public class ProgressingStartedMiddleware(
    RequestDelegate next, 
    IMemoryCache memoryCache, 
    ILogger<ProgressingStartedMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            var correlationId = context.GetCorrelationId();
            if (correlationId == null)
            {
                context.SetCorrelationId(Guid.NewGuid());
            }
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            if (!memoryCache.TryGetValue(nameof(SnapshotLog), out Guid latestSnapshotId))
            {
                var snapshotLogRepository = context.RequestServices.GetRequiredService<ISnapshotLogRepository>();
                var matchedSnapshotLog = await snapshotLogRepository.GetLatestSnapshotLogAsync();
                latestSnapshotId = matchedSnapshotLog?.Id ?? Guid.Empty;
                memoryCache.Set(nameof(SnapshotLog), latestSnapshotId);
            }

            context.SetSnapshotId(latestSnapshotId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while setting snapshot id");
        }

        await next(context);
    }
}