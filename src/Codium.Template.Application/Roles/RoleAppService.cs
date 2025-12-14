using AutoMapper;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Roles;

public class RoleAppService(
    IRoleRepository roleRepository,
    IRolePermissionRepository rolePermissionRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<ApplicationResource> localizer)
    : IRoleAppService
{
    public async Task<RoleResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            include: q => q
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)!,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return new RoleResponseDto
        {
            Id = matchedRole.Id,
            Name = matchedRole.Name,
            Description = matchedRole.Description,
            Permissions = matchedRole.RolePermissions.Select(rp => new PermissionResponseDto
            {
                Id = rp.Permission!.Id,
                Name = rp.Permission!.Name
            }).ToList()
        };
    }

    public async Task<List<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var matchedRoles = await roleRepository.GetAllAsync(
            orderBy: q => q.OrderBy(r => r.NormalizedName),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        return mapper.Map<List<RoleResponseDto>>(matchedRoles);
    }

    public async Task<PagedResult<RoleResponseDto>> GetPageableAndFilterAsync(GetListRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        var pagedRoles = await roleRepository.GetListSortedAsync(
            page: request.Page,
            perPage: request.PerPage,
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? r => r.NormalizedName.Contains(request.Search.NormalizeValue())
                : null,
            sorts: request.ToSortRequests(),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        var mappedRoles = mapper.Map<List<RoleResponseDto>>(pagedRoles.Data);
        
        return new PagedResult<RoleResponseDto>(mappedRoles, pagedRoles.TotalCount, pagedRoles.Page, pagedRoles.PerPage);
    }

    public async Task<RoleResponseDto> CreateAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingRole = await roleRepository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken);
        if (existingRole)
        {
            throw new AppConflictException(localizer["RoleAppService:CreateAsync:Exists", request.Name]);
        }
        
        var newRole = new Role
        {
            Name = request.Name,
            NormalizedName = request.Name.NormalizeValue(),
            Description = request.Description
        };
        
        newRole = await roleRepository.AddAsync(newRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<RoleResponseDto>(newRole);
    }

    public async Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var existingRole = await roleRepository.ExistsByNameAsync(request.Name, matchedRole.Id, cancellationToken);
        if (existingRole)
        {
            throw new AppConflictException(localizer["RoleAppService:UpdateAsync:Exists", request.Name]);
        }
        
        matchedRole.Name = request.Name;
        matchedRole.NormalizedName = request.Name.NormalizeValue();
        matchedRole.Description = request.Description;

        matchedRole = await roleRepository.UpdateAsync(matchedRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<RoleResponseDto>(matchedRole);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await roleRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermission = await permissionRepository.GetAsync(
            predicate: p => p.Id == permissionId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var existRolePermission = await rolePermissionRepository.AnyAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                rp.PermissionId == matchedPermission.Id,
            cancellationToken: cancellationToken
        );

        if (existRolePermission)
        {
            throw new AppConflictException(localizer["RoleAppService:AddToPermissionAsync:Exists"]);
        }
        
        var newRolePermission = new RolePermission
        {
            RoleId = matchedRole.Id,
            PermissionId = matchedPermission.Id
        };
        
        await rolePermissionRepository.AddAsync(newRolePermission, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermissions = await permissionRepository.GetAllAsync(
            predicate: p => permissionIds.Contains(p.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedPermissions.Count != permissionIds.Count)
        {
            throw new AppEntityNotFoundException(localizer["RoleAppService:AddToPermissionsAsync:MissingPermission"]);
        }
        
        var existRolePermissions = await rolePermissionRepository.GetAllAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                permissionIds.Contains(rp.Permission!.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (existRolePermissions.Count != 0)
        {
            throw new AppConflictException(localizer["RoleAppService:AddToPermissionsAsync:Exists"]);
        }
        
        var newRolePermissions = matchedPermissions.Select(p => new RolePermission
        {
            RoleId = matchedRole.Id,
            PermissionId = p.Id
        }).ToList();
        
        await rolePermissionRepository.AddRangeAsync(newRolePermissions, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermission = await permissionRepository.GetAsync(
            predicate: p => p.Id == permissionId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
    
        var matchedRolePermission = await rolePermissionRepository.GetAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                rp.PermissionId == matchedPermission.Id,
            cancellationToken: cancellationToken
        );
    
        if (matchedRolePermission == null)
        {
            throw new AppConflictException(localizer["RoleAppService:RemoveFromPermissionAsync:NotFound"]);
        }

        await rolePermissionRepository.DeleteAsync(matchedRolePermission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermissions = await permissionRepository.GetAllAsync(
            predicate: p => permissionIds.Contains(p.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        if (matchedPermissions.Count != permissionIds.Count)
        {
            throw new AppEntityNotFoundException(localizer["RoleAppService:RemoveFromPermissionsAsync:MissingPermissions"]);
        }

        var matchedRolePermissions = await rolePermissionRepository.GetAllAsync(
            predicate: rp =>
                rp.RoleId == matchedRole.Id &&
                matchedPermissions.Select(p => p.Id).Contains(rp.PermissionId),
            cancellationToken: cancellationToken
        );

        if (matchedRolePermissions.Count != matchedPermissions.Count)
        {
            throw new AppConflictException(localizer["RoleAppService:RemoveFromPermissionsAsync:MissingRolePermissions"]);
        }

        await rolePermissionRepository.DeleteRangeAsync(matchedRolePermissions, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}