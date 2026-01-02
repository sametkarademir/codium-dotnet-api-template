namespace Codium.Template.Application.Contracts.Auth;

public interface IAuthAppService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task ResendEmailConfirmationLinkAsync(string email, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
}