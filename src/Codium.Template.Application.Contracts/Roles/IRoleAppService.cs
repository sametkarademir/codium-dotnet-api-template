using Codium.Template.Application.Contracts.Common.Results;

namespace Codium.Template.Application.Contracts.Roles;

public interface IRoleAppService
{
    Task<RoleResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<RoleResponseDto>> GetPageableAndFilterAsync(GetListRolesRequestDto request, CancellationToken cancellationToken = default);
    Task<RoleResponseDto> CreateAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task AddToPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default);
    Task AddToPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default);
    Task RemoveFromPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default);
    Task RemoveFromPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default);
}