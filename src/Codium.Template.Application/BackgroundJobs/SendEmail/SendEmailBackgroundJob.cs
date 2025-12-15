using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.SendEmail;

namespace Codium.Template.Application.BackgroundJobs.SendEmail;

public class SendEmailBackgroundJob : IBackgroundJob<SendEmailBackgroundJobArgs>
{
    public async Task Execute(SendEmailBackgroundJobArgs args, CancellationToken cancellationToken = default)
    {
        //TODO: Implement email sending logic here.
        await Task.CompletedTask;
    }
}