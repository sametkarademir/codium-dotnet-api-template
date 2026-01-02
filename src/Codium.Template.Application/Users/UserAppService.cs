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
using Codium.Template.Domain.Shared.Querying;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Users;

public class UserAppService : IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordValidator _passwordValidator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobExecutor _backgroundJobExecutor;
    private readonly IStringLocalizer<UserAppService> _localizer;

    public UserAppService(IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork,
        IPasswordValidator passwordValidator,
        IPasswordHasher<User> passwordHasher,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IBackgroundJobExecutor backgroundJobExecutor,
        IStringLocalizer<UserAppService> localizer)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _passwordValidator = passwordValidator;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _backgroundJobExecutor = backgroundJobExecutor;
        _localizer = localizer;
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
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
        var matchedUsers = await _userRepository.GetListAsync(
            orderBy: q => q.OrderBy(r => r.NormalizedEmail),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        return _mapper.Map<List<UserResponseDto>>(matchedUsers);
    }

    public async Task<PagedResult<UserResponseDto>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        var pagedUsers = await _userRepository.GetListSortedAsync(
            page: request.Page,
            perPage: request.PerPage,
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? u => 
                    u.NormalizedEmail.Contains(request.Search.NormalizeValue())
                : null,
            sort: new SortRequest(request.Field, request.Order),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var mappedUsers = _mapper.Map<List<UserResponseDto>>(pagedUsers.Data);

        return new PagedResult<UserResponseDto>(mappedUsers, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PerPage);
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken);
        if (existingUser)
        {
            throw new AppConflictException(_localizer["UserAppService:CreateAsync:Exists", request.Email]);
        }

        var passwordValidatorResult = _passwordValidator.Validate(request.Password);
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
        
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);
        newUser.LockoutEnabled = UserConsts.AllowedForNewUsers;
        
        newUser = await _userRepository.AddAsync(newUser, cancellationToken);
            
        return _mapper.Map<UserResponseDto>(newUser);
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        matchedUser.PhoneNumber = request.PhoneNumber;
        matchedUser.FirstName = request.FirstName;
        matchedUser.LastName = request.LastName;
        matchedUser.IsActive = request.IsActive;

        matchedUser =  await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<UserResponseDto>(matchedUser);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        var existingUserRole = await _userRoleRepository.AnyAsync(
            predicate: ur => 
                ur.UserId == matchedUser.Id && 
                ur.RoleId == matchedRole.Id,
            cancellationToken: cancellationToken
        );
        
        if (existingUserRole)
        {
            throw new AppConflictException(_localizer["UserAppService:AddToRoleAsync:Exists"]);
        }
        
        var newUserRole = new UserRole
        {
            UserId = matchedUser.Id,
            RoleId = matchedRole.Id
        };
        
        await _userRoleRepository.AddAsync(newUserRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRoles = await _roleRepository.GetAllAsync(
            predicate: r => roleIds.Contains(r.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedRoles.Count != roleIds.Count)
        {
            throw new AppEntityNotFoundException(_localizer["UserAppService:AddToRolesAsync:MissingRoles"]);
        }
        
        var existUserRoles = await _userRoleRepository.GetAllAsync(
            predicate: ur => 
                ur.RoleId == id && 
                roleIds.Contains(ur.Role!.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (existUserRoles.Count != 0)
        {
            throw new AppConflictException(_localizer["UserAppService:AddToRolesAsync:Exists"]);
        }
        
        var newUserRoles = matchedRoles.Select(p => new UserRole
        {
            RoleId = p.Id,
            UserId = matchedUser.Id
        }).ToList();
        
        await _userRoleRepository.AddRangeAsync(newUserRoles, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedUserRole = await _userRoleRepository.GetAsync(
            predicate: ur =>
                ur.RoleId == matchedRole.Id &&
                ur.UserId == matchedUser.Id,
            cancellationToken: cancellationToken
        );
        
        if (matchedUserRole == null)
        {
            throw new AppConflictException(_localizer["UserAppService:RemoveFromRoleAsync:NotFound"]);
        }
        
        await _userRoleRepository.DeleteAsync(matchedUserRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRolesAsync(Guid id, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRoles =  await _roleRepository.GetAllAsync(
            predicate: r => roleIds.Contains(r.Id),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        if (matchedRoles.Count != roleIds.Count)
        {
            throw new AppEntityNotFoundException(_localizer["UserAppService:RemoveFromRolesAsync:MissingRoles"]);
        }
        
        var matchedUserRoles = await _userRoleRepository.GetAllAsync(
            predicate: ur =>
                ur.UserId == matchedUser.Id &&
                matchedRoles.Select(r => r.Id).Contains(ur.RoleId),
            cancellationToken: cancellationToken
        );
        
        if (matchedUserRoles.Count != matchedRoles.Count)
        {
            throw new AppConflictException(_localizer["UserAppService:RemoveFromRolesAsync:MissingRolePermissions"]);
        }
        
        await _userRoleRepository.DeleteRangeAsync(matchedUserRoles, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.EmailConfirmed = !matchedUser.EmailConfirmed;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.PhoneNumberConfirmed = !matchedUser.PhoneNumberConfirmed;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.TwoFactorEnabled = !matchedUser.TwoFactorEnabled;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.IsActive = !matchedUser.IsActive;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LockAsync(Guid id, DateTimeOffset? lockoutEnd = null, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = lockoutEnd ?? DateTimeOffset.UtcNow.Add(UserConsts.DefaultLockoutTimeSpanMinutes);
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = null;
        matchedUser.AccessFailedCount = 0;
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var passwordValidationResult = _passwordValidator.Validate(request.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            throw new AppValidationException(passwordValidationResult.Errors);
        }
        
        matchedUser.PasswordHash = _passwordHasher.HashPassword(matchedUser, request.NewPassword);
        matchedUser.PasswordChangedTime = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
            new InvalidateAllSessionsBackgroundJobArgs
            {
                UserId = matchedUser.Id,
                Reason = "Password reset by admin",
                CorrelationId = _httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid()
            }
        );
    }
}