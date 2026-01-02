namespace Codium.Template.Application.Contracts.BackgroundJobs.SendEmail;

public class SendEmailBackgroundJobArgs
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    
    public Guid CorrelationId { get; set; }
}