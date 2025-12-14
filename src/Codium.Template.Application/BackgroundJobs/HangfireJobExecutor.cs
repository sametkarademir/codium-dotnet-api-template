using System.Text.Json;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Domain.Shared.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.BackgroundJobs;

public class HangfireJobExecutor(ILogger<HangfireJobExecutor> logger) : IBackgroundJobExecutor
{
    public void Enqueue<TJob, TParameter>(TParameter args, CancellationToken cancellationToken = default) 
        where TJob : IBackgroundJob<TParameter> where TParameter : class
    {
        var logDetails = new Dictionary<string, object>()
        {
            { "JobName", typeof(TJob).Name },
            { "Parameters", JsonSerializer.Serialize(args) }
        };
        
        logger
            .WithProperties()
            .AddRange(logDetails)
            .LogInformation("Enqueue job");

        try
        {
            BackgroundJob.Enqueue<TJob>(job => job.Execute(args, cancellationToken));
        }
        catch (Exception e)
        {
            logger
                .WithProperties()
                .AddRange(logDetails)
                .LogError("Failed to enqueue job", e);
            
            throw;
        }
    }

    public void Schedule<TJob, TParameter>(TParameter args, TimeSpan delay, CancellationToken cancellationToken = default) 
        where TJob : IBackgroundJob<TParameter> where TParameter : class
    {
        var logDetails = new Dictionary<string, object>()
        {
            { "JobName", typeof(TJob).Name },
            { "Parameters", JsonSerializer.Serialize(args) },
            { "DelayInMinutes", delay.TotalMinutes }
        };
        
        logger
            .WithProperties()
            .AddRange(logDetails)
            .LogInformation("Schedule job");

        try
        {
            BackgroundJob.Schedule<TJob>(job => job.Execute(args, cancellationToken), delay);
        }
        catch (Exception e)
        {
            logger
                .WithProperties()
                .AddRange(logDetails)
                .LogError("Failed to schedule job", e);
            
            throw;
        }
    }
}