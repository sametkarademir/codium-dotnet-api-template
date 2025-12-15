using AutoMapper;
using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Users;

public class UserAppService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork,
    IPasswordValidator passwordValidator,
    IPasswordHasher<User> passwordHasher,
    IOptions<IdentityUserOptions> options,
    IStringLocalizer<UserAppService> localizer,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper,
    IBackgroundJobExecutor backgroundJobExecutor)
    : IUserAppService
{
    private readonly IdentityUserOptions _identityUserOptions = options.Value;

    public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            include: q => q
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)!,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return new UserResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email,
            EmailConfirmed = matchedUser.EmailConfirmed,
            PhoneNumber = matchedUser.PhoneNumber,
            PhoneNumberConfirmed = matchedUser.PhoneNumberConfirmed,
            TwoFactorEnabled = matchedUser.TwoFactorEnabled,
            LockoutEnd = matchedUser.LockoutEnd,
            LockoutEnabled = matchedUser.LockoutEnabled,
            AccessFailedCount = matchedUser.AccessFailedCount,
            FirstName = matchedUser.FirstName,
            LastName = matchedUser.LastName,
            PasswordChangedTime = matchedUser.PasswordChangedTime,
            IsActive = matchedUser.IsActive,
            Roles = matchedUser.UserRoles.Select(ur => new RoleResponseDto
            {
                Id = ur.Role!.Id,
                Name = ur.Role!.Name
            }).ToList()
        };
    }

    public async Task<List<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var matchedUsers = await userRepository.GetListAsync(
            orderBy: q => q.OrderBy(r => r.NormalizedEmail),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        return mapper.Map<List<UserResponseDto>>(matchedUsers);
    }

    public async Task<PagedResult<UserResponseDto>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        var pagedUsers = await userRepository.GetListSortedAsync(
            page: request.Page,
            perPage: request.PerPage,
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? u => 
                    u.NormalizedEmail.Contains(request.Search.NormalizeValue())
                : null,
            sorts: request.ToSortRequests(),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var mappedUsers = mapper.Map<List<UserResponseDto>>(pagedUsers.Data);

        return new PagedResult<UserResponseDto>(mappedUsers, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PerPage);
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken);
        if (existingUser)
        {
            throw new AppConflictException(localizer["UserAppService:CreateAsync:Exists", request.Email]);
        }

        var passwordValidatorResult = passwordValidator.Validate(_identityUserOptions.Password, request.Password);
        if (!passwordValidatorResult.Succeeded)
        {
            throw new AppValidationException(passwordValidatorResult.Errors);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = request.Email.NormalizeValue(),
            EmailConfirmed = request.EmailConfirmed,
            PhoneNumber = request.PhoneNumber,
            PhoneNumberConfirmed = request.PhoneNumberConfirmed,
            TwoFactorEnabled = request.TwoFactorEnabled,
            LockoutEnd = null,
            AccessFailedCount = 0,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive
        };
        
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        newUser.LockoutEnabled = _identityUserOptions.Lockout.AllowedForNewUsers;
        
        newUser = await userRepository.AddAsync(newUser, cancellationToken);
            
        return mapper.Map<UserResponseDto>(newUser);
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        matchedUser.PhoneNumber = request.PhoneNumber;
        matchedUser.FirstName = request.FirstName;
        matchedUser.LastName = request.LastName;
        matchedUser.IsActive = request.IsActive;

        matchedUser =  await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<UserResponseDto>(matchedUser);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await userRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        var existingUserRole = await userRoleRepository.AnyAsync(
            predicate: ur => 
                ur.UserId == matchedUser.Id && 
                ur.RoleId == matchedRole.Id,
            cancellationToken: cancellationToken
        );
        
        if (existingUserRole)
        {
            throw new AppConflictException(localizer["UserAppService:AddToRoleAsync:Exists"]);
        }
        
        var newUserRole = new UserRole
        {
            UserId = matchedUser.Id,
            RoleId = matchedRole.Id
        };
        
        await userRoleRepository.AddAsync(newUserRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRoles = await roleRepository.GetAllAsync(
            predicate: r => roleIds.Contains(r.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedRoles.Count != roleIds.Count)
        {
            throw new AppEntityNotFoundException(localizer["UserAppService:AddToRolesAsync:MissingRoles"]);
        }
        
        var existUserRoles = await userRoleRepository.GetAllAsync(
            predicate: ur => 
                ur.RoleId == id && 
                roleIds.Contains(ur.Role!.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (existUserRoles.Count != 0)
        {
            throw new AppConflictException(localizer["UserAppService:AddToRolesAsync:Exists"]);
        }
        
        var newUserRoles = matchedRoles.Select(p => new UserRole
        {
            RoleId = p.Id,
            UserId = matchedUser.Id
        }).ToList();
        
        await userRoleRepository.AddRangeAsync(newUserRoles, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRole = await roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedUserRole = await userRoleRepository.GetAsync(
            predicate: ur =>
                ur.RoleId == matchedRole.Id &&
                ur.UserId == matchedUser.Id,
            cancellationToken: cancellationToken
        );
        
        if (matchedUserRole == null)
        {
            throw new AppConflictException(localizer["UserAppService:RemoveFromRoleAsync:NotFound"]);
        }
        
        await userRoleRepository.DeleteAsync(matchedUserRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRoles =  await roleRepository.GetAllAsync(
            predicate: r => roleIds.Contains(r.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedRoles.Count != roleIds.Count)
        {
            throw new AppEntityNotFoundException(localizer["UserAppService:RemoveFromRolesAsync:MissingRoles"]);
        }
        
        var matchedUserRoles = await userRoleRepository.GetAllAsync(
            predicate: ur =>
                ur.UserId == matchedUser.Id &&
                matchedRoles.Select(r => r.Id).Contains(ur.RoleId),
            cancellationToken: cancellationToken
        );
        
        if (matchedUserRoles.Count != matchedRoles.Count)
        {
            throw new AppConflictException(localizer["UserAppService:RemoveFromRolesAsync:MissingRolePermissions"]);
        }
        
        await userRoleRepository.DeleteRangeAsync(matchedUserRoles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.EmailConfirmed = !matchedUser.EmailConfirmed;
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.PhoneNumberConfirmed = !matchedUser.PhoneNumberConfirmed;
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.TwoFactorEnabled = !matchedUser.TwoFactorEnabled;
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.IsActive = !matchedUser.IsActive;
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LockAsync(Guid id, DateTimeOffset? lockoutEnd = null, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = lockoutEnd ?? DateTimeOffset.UtcNow.Add(_identityUserOptions.Lockout.DefaultLockoutTimeSpanMinutes);
        
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = null;
        matchedUser.AccessFailedCount = 0;
        
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var passwordValidationResult = passwordValidator.Validate(_identityUserOptions.Password, request.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            throw new AppValidationException(passwordValidationResult.Errors);
        }
        
        matchedUser.PasswordHash = passwordHasher.HashPassword(matchedUser, request.NewPassword);
        matchedUser.PasswordChangedTime = DateTime.UtcNow;
        
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
            new InvalidateAllSessionsBackgroundJobArgs
            {
                UserId = matchedUser.Id,
                CorrelationId = httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid(),
                Reason = "Password reset by admin"
            },
            cancellationToken);
    }
}