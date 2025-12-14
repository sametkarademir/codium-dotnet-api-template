using Codium.Template.Application.Contracts.Common.Results;

namespace Codium.Template.Application.Contracts.Profiles;

public interface IProfileAppService
{
    Task<ProfileResponseDto> GetProfileAsync(CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default);
    
    Task<PagedResult<SessionResponseDto>> GetPageableAndFilterAsync(GetListSessionsRequestDto request, CancellationToken cancellationToken = default);
    Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}