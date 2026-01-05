using Codium.Template.Application.Contracts.Email;
using Codium.Template.Domain.Shared.Email;
using Codium.Template.Domain.Shared.Extensions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Codium.Template.Application.Email;

public class EmailAppService : IEmailAppService
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<EmailAppService> _logger;

    public EmailAppService(
        IOptions<SmtpOptions> smtpOptions,
        ILogger<EmailAppService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var logDetail = new Dictionary<string, object>
        {
            { "Service", nameof(EmailAppService) },
            { "ServiceMethod", nameof(SendEmailAsync) },
            { "To", request.To },
            { "Subject", request.Subject },
            { "IsHtml", request.IsHtml }
        };

        _logger
            .WithProperties()
            .AddRange(logDetail)
            .LogInformation("Starting to send email");

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
            message.To.Add(new MailboxAddress(request.To, request.To));
            message.Subject = request.Subject;

            var bodyBuilder = new BodyBuilder();
            if (request.IsHtml)
            {
                bodyBuilder.HtmlBody = request.Body;
            }
            else
            {
                bodyBuilder.TextBody = request.Body;
            }
            
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, _smtpOptions.EnableSsl, cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(_smtpOptions.UserName) && !string.IsNullOrWhiteSpace(_smtpOptions.Password))
            {
                await client.AuthenticateAsync(_smtpOptions.UserName, _smtpOptions.Password, cancellationToken);
            }
            
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger
                .WithProperties()
                .AddRange(logDetail)
                .LogError("Failed to send email", ex);
            
            throw;
        }
    }
}

