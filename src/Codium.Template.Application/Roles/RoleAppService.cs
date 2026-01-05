using AutoMapper;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Roles;

public class RoleAppService : IRoleAppService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<ApplicationResource> _localizer;


    public RoleAppService(
        IRoleRepository roleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository, 
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStringLocalizer<ApplicationResource> localizer)
    {
        _roleRepository = roleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<RoleResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
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
        var matchedRoles = await _roleRepository.GetAllAsync(
            orderBy: q => q.OrderBy(r => r.NormalizedName),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        return _mapper.Map<List<RoleResponseDto>>(matchedRoles);
    }

    public async Task<PagedResult<RoleResponseDto>> GetPageableAndFilterAsync(GetListRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        var pagedRoles = await _roleRepository.GetListSortedAsync(
            page: request.Page,
            perPage: request.PerPage,
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? r => r.NormalizedName.Contains(request.Search.NormalizeValue())
                : null,
            sort: request.GetSortRequest(nameof(CreationAuditedEntity.CreationTime)),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        var mappedRoles = _mapper.Map<List<RoleResponseDto>>(pagedRoles.Data);
        
        return new PagedResult<RoleResponseDto>(mappedRoles, pagedRoles.TotalCount, pagedRoles.Page, pagedRoles.PerPage);
    }

    public async Task<RoleResponseDto> CreateAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleRepository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken);
        if (existingRole)
        {
            throw new AppConflictException(_localizer["RoleAppService:CreateAsync:Exists", request.Name]);
        }
        
        var newRole = new Role
        {
            Name = request.Name,
            NormalizedName = request.Name.NormalizeValue(),
            Description = request.Description
        };
        
        newRole = await _roleRepository.AddAsync(newRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<RoleResponseDto>(newRole);
    }

    public async Task<RoleResponseDto> UpdateAsync(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var existingRole = await _roleRepository.ExistsByNameAsync(request.Name, matchedRole.Id, cancellationToken);
        if (existingRole)
        {
            throw new AppConflictException(_localizer["RoleAppService:UpdateAsync:Exists", request.Name]);
        }
        
        matchedRole.Name = request.Name;
        matchedRole.NormalizedName = request.Name.NormalizeValue();
        matchedRole.Description = request.Description;

        matchedRole = await _roleRepository.UpdateAsync(matchedRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<RoleResponseDto>(matchedRole);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _roleRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermission = await _permissionRepository.GetAsync(
            predicate: p => p.Id == permissionId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var existRolePermission = await _rolePermissionRepository.AnyAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                rp.PermissionId == matchedPermission.Id,
            cancellationToken: cancellationToken
        );

        if (existRolePermission)
        {
            throw new AppConflictException(_localizer["RoleAppService:AddToPermissionAsync:Exists"]);
        }
        
        var newRolePermission = new RolePermission
        {
            RoleId = matchedRole.Id,
            PermissionId = matchedPermission.Id
        };
        
        await _rolePermissionRepository.AddAsync(newRolePermission, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermissions = await _permissionRepository.GetAllAsync(
            predicate: p => permissionIds.Contains(p.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedPermissions.Count != permissionIds.Count)
        {
            throw new AppEntityNotFoundException(_localizer["RoleAppService:AddToPermissionsAsync:MissingPermission"]);
        }
        
        var existRolePermissions = await _rolePermissionRepository.GetAllAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                permissionIds.Contains(rp.Permission!.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (existRolePermissions.Count != 0)
        {
            throw new AppConflictException(_localizer["RoleAppService:AddToPermissionsAsync:Exists"]);
        }
        
        var newRolePermissions = matchedPermissions.Select(p => new RolePermission
        {
            RoleId = matchedRole.Id,
            PermissionId = p.Id
        }).ToList();
        
        await _rolePermissionRepository.AddRangeAsync(newRolePermissions, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromPermissionAsync(Guid id, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermission = await _permissionRepository.GetAsync(
            predicate: p => p.Id == permissionId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
    
        var matchedRolePermission = await _rolePermissionRepository.GetAsync(
            predicate: rp => 
                rp.RoleId == matchedRole.Id && 
                rp.PermissionId == matchedPermission.Id,
            cancellationToken: cancellationToken
        );
    
        if (matchedRolePermission == null)
        {
            throw new AppConflictException(_localizer["RoleAppService:RemoveFromPermissionAsync:NotFound"]);
        }

        await _rolePermissionRepository.DeleteAsync(matchedRolePermission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromPermissionsAsync(Guid id, List<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedPermissions = await _permissionRepository.GetAllAsync(
            predicate: p => permissionIds.Contains(p.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        if (matchedPermissions.Count != permissionIds.Count)
        {
            throw new AppEntityNotFoundException(_localizer["RoleAppService:RemoveFromPermissionsAsync:MissingPermissions"]);
        }

        var matchedRolePermissions = await _rolePermissionRepository.GetAllAsync(
            predicate: rp =>
                rp.RoleId == matchedRole.Id &&
                matchedPermissions.Select(p => p.Id).Contains(rp.PermissionId),
            cancellationToken: cancellationToken
        );

        if (matchedRolePermissions.Count != matchedPermissions.Count)
        {
            throw new AppConflictException(_localizer["RoleAppService:RemoveFromPermissionsAsync:MissingRolePermissions"]);
        }

        await _rolePermissionRepository.DeleteRangeAsync(matchedRolePermissions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncPermissionsAsync(Guid id, SyncRolePermissionsRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var matchedRole = await _roleRepository.GetAsync(
                predicate: r => r.Id == id,
                include: q => q.Include(r => r.RolePermissions),
                enableTracking: true,
                cancellationToken: cancellationToken
            );

 
            var currentPermissionIds = matchedRole.RolePermissions.Select(rp => rp.PermissionId).ToList();
            
       
            var permissionsToAdd = request.PermissionIds.Except(currentPermissionIds).ToList();
            var permissionsToRemove = currentPermissionIds.Except(request.PermissionIds).ToList();

          
            if (permissionsToAdd.Any())
            {
                var existingPermissions = await _permissionRepository.GetAllAsync(
                    predicate: p => permissionsToAdd.Contains(p.Id),
                    enableTracking: false,
                    cancellationToken: cancellationToken
                );

                if (existingPermissions.Count != permissionsToAdd.Count)
                {
                    throw new AppEntityNotFoundException(_localizer["RoleAppService:SyncPermissionsAsync:MissingPermissions"]);
                }

           
                var newRolePermissions = permissionsToAdd.Select(permissionId => new RolePermission
                {
                    RoleId = matchedRole.Id,
                    PermissionId = permissionId
                }).ToList();

                await _rolePermissionRepository.AddRangeAsync(newRolePermissions, cancellationToken: cancellationToken);
            }

          
            if (permissionsToRemove.Any())
            {
                var rolePermissionsToRemove = matchedRole.RolePermissions
                    .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                    .ToList();

                await _rolePermissionRepository.DeleteRangeAsync(rolePermissionsToRemove, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}