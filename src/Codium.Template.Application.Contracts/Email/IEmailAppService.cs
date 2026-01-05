namespace Codium.Template.Application.Contracts.Email;

public interface IEmailAppService
{
    Task SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default);
}