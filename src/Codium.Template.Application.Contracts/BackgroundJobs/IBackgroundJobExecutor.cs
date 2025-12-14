namespace Codium.Template.Application.Contracts.BackgroundJobs;

public interface IBackgroundJobExecutor
{
    void Enqueue<TJob, TParameter>(TParameter args, CancellationToken cancellationToken = default) 
        where TJob : IBackgroundJob<TParameter> where TParameter : class;
    
    void Schedule<TJob, TParameter>(TParameter args, TimeSpan delay, CancellationToken cancellationToken = default) 
        where TJob : IBackgroundJob<TParameter> where TParameter : class;
}