using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.SendEmail;
using Codium.Template.Application.Contracts.Email;
using Codium.Template.Domain.Shared.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.BackgroundJobs.SendEmail;

public class SendEmailBackgroundJob : IBackgroundJob<SendEmailBackgroundJobArgs>
{
    private readonly IEmailAppService _emailService;
    private readonly ILogger<SendEmailBackgroundJob> _logger;

    public SendEmailBackgroundJob(IEmailAppService emailService, ILogger<SendEmailBackgroundJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Execute(SendEmailBackgroundJobArgs args, IJobCancellationToken cancellationToken)
    {
        var logDetail = new Dictionary<string, object>
        {
            { "Service", nameof(SendEmailBackgroundJob) },
            { "ServiceMethod", nameof(Execute) },
            { "CorrelationId", args.CorrelationId },
            { "To", args.To },
            { "Subject", args.Subject }
        };
        
        _logger
            .WithProperties()
            .AddRange(logDetail)
            .LogInformation("Starting SendEmailBackgroundJob");

        try
        {
            await _emailService.SendEmailAsync(new SendEmailRequestDto
            {
                To = args.To,
                Subject = args.Subject,
                Body = args.Body,
                IsHtml = args.IsHtml
            }, cancellationToken.ShutdownToken);
            
            _logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation("SendEmailBackgroundJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger
                .WithProperties()
                .AddRange(logDetail)
                .LogError("SendEmailBackgroundJob failed", ex);
            
            throw;
        }
    }
}