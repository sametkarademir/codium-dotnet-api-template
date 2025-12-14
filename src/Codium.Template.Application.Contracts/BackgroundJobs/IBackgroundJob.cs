namespace Codium.Template.Application.Contracts.BackgroundJobs;

public interface IBackgroundJob<in TParameter> where TParameter : class
{
    Task Execute(TParameter args, CancellationToken cancellationToken = default);
}