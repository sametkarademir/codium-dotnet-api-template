using AutoMapper;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Domain.Repositories;

namespace Codium.Template.Application.Permissions;

public class PermissionAppService(
    IPermissionRepository permissionRepository,
    IMapper mapper)
    : IPermissionAppService
{
    public async Task<List<PermissionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var matchedPermissions = await permissionRepository.GetAllAsync(
            orderBy: q => q.OrderBy(p => p.NormalizedName),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return mapper.Map<List<PermissionResponseDto>>(matchedPermissions);
    }
}