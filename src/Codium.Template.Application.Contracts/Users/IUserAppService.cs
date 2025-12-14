using Codium.Template.Application.Contracts.Common.Results;

namespace Codium.Template.Application.Contracts.Users;

public interface IUserAppService
{
    Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<UserResponseDto>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddToRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default);
    Task AddToRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default);
    Task RemoveFromRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default);
    Task RemoveFromRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default);

    Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default);
    Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default);
    Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default);
    Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task LockAsync(Guid id, DateTimeOffset? lockoutEnd = null, CancellationToken cancellationToken = default);
    Task UnlockAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default);
}