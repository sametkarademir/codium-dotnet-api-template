using Codium.Template.Application.Contracts.Common.Results;

namespace Codium.Template.Application.Contracts.Sessions;

public interface ISessionAppService
{
    Task<PagedResult<SessionResponseDto>> GetPageableAndFilterAsync(GetListSessionsRequestDto request, CancellationToken cancellationToken = default);
    Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}