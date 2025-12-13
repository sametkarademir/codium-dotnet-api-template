using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Domain;

public class DevelopmentDataSeederContributor
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<DevelopmentDataSeederContributor> _logger;

    public DevelopmentDataSeederContributor(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        ILogger<DevelopmentDataSeederContributor> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting seeding...");

            // Add your seeding logic here
            await CreateAllPermissionsAsync();
            await CreateAdminUserAsync();

            _logger.LogInformation("Seeding completed.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred during seeding");
        }
    }

    private async Task CreateAllPermissionsAsync()
    {
        var permissionCount = await _permissionRepository.CountAsync();
        if (permissionCount > 0)
        {
            return;
        }

        var permissions = GetAllPermissionsFromConsts();
        await _permissionRepository.AddRangeAsync(permissions);
        await _unitOfWork.SaveChangesAsync();
    }

    private List<Permission> GetAllPermissionsFromConsts()
    {
        var permissions = new List<Permission>();
        var permissionConstType = typeof(PermissionConsts);

        var nestedTypes = permissionConstType.GetNestedTypes();

        foreach (var nestedType in nestedTypes)
        {
            var fields = nestedType.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var value = (string)field.GetValue(null)!;
                    permissions.Add(new Permission
                    {
                        Name = value,
                    });
                }
            }
        }

        return permissions;
    }

    private async Task CreateAdminUserAsync()
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            const string adminRoleName = "Admin";
            var normalizedAdminRoleName = adminRoleName.NormalizeValue();

            const string userName = "admin@codium.com";
            var normalizedUserName = userName.NormalizeValue();

            var matchedAdminUser = await _userRepository.SingleOrDefaultAsync(
                predicate: u => u.UserName == userName,
                enableTracking: false
            );

            if (matchedAdminUser != null)
            {
                return;
            }

            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = adminRoleName,
                NormalizedName = normalizedAdminRoleName
            };
            await _roleRepository.AddAsync(newRole);

            var allPermissions = await _permissionRepository.GetAllAsync(enableTracking: false);
            var newRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = newRole.Id,
                PermissionId = p.Id
            }).ToList();

            await _rolePermissionRepository.AddRangeAsync(newRolePermissions);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                NormalizedUserName = normalizedUserName,
                Email = userName,
                NormalizedEmail = normalizedUserName,
                EmailConfirmed = true,
                PhoneNumber = null,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                FirstName = "Admin",
                LastName = "User",
                PasswordChangedTime = null,
                IsActive = true
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, "Pp123456*");
            await _userRepository.AddAsync(newUser);

            var newUserRole = new UserRole
            {
                RoleId = newRole.Id,
                UserId = newUser.Id
            };
            await _userRoleRepository.AddAsync(newUserRole);

            await transaction.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error occurred while creating admin user");

            throw;
        }
    }
}