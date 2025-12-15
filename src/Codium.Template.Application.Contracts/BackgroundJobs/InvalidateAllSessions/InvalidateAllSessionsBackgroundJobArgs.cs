namespace Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;

public class InvalidateAllSessionsBackgroundJobArgs
{
    public Guid UserId { get; set; }
    public Guid CorrelationId { get; set; }
    public string? Reason { get; set; }
}

