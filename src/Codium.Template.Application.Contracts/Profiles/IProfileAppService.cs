namespace Codium.Template.Application.Contracts.Profiles;

public interface IProfileAppService
{
    Task<ProfileResponseDto> GetProfileAsync(CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default);
}