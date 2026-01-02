using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.SendEmail;
using Hangfire;

namespace Codium.Template.Application.BackgroundJobs.SendEmail;

public class SendEmailBackgroundJob : IBackgroundJob<SendEmailBackgroundJobArgs>
{
    public async Task Execute(SendEmailBackgroundJobArgs args, IJobCancellationToken cancellationToken)
    {
        //TODO: Implement email sending logic here.
        await Task.CompletedTask;
    }
}