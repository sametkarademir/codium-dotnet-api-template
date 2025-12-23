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

public class DevelopmentDataSeederContributor(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher,
    ILogger<DevelopmentDataSeederContributor> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            logger.LogInformation("Starting seeding...");

            // Add your seeding logic here
            await CreateAllPermissionsAsync();
            await CreateAdminUserAsync();

            logger.LogInformation("Seeding completed.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred during seeding");
        }
    }

    private async Task CreateAllPermissionsAsync()
    {
        var permissionCount = await permissionRepository.CountAsync();
        if (permissionCount > 0)
        {
            return;
        }

        var permissions = GetAllPermissionsFromConsts();
        await permissionRepository.AddRangeAsync(permissions);
        await unitOfWork.SaveChangesAsync();
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
                        NormalizedName = value.NormalizeValue()
                    });
                }
            }
        }

        return permissions;
    }

    private async Task CreateAdminUserAsync()
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            const string adminRoleName = "Admin";
            var normalizedAdminRoleName = adminRoleName.NormalizeValue();

            const string email = "admin@codium.com";
            var normalizedEmail = email.NormalizeValue();

            var matchedAdminUser = await userRepository.SingleOrDefaultAsync(
                predicate: u => u.Email == email,
                enableTracking: false
            );

            if (matchedAdminUser != null)
            {
                return;
            }

            var existingAdminRole = await roleRepository.SingleOrDefaultAsync(
                predicate: r => r.NormalizedName == normalizedAdminRoleName,
                enableTracking: false
            );
            if (existingAdminRole == null)
            {
                existingAdminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = adminRoleName,
                    NormalizedName = normalizedAdminRoleName
                };
                await roleRepository.AddAsync(existingAdminRole);
            }

            var allPermissions = await permissionRepository.GetAllAsync(enableTracking: false);
            var newRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = existingAdminRole.Id,
                PermissionId = p.Id
            }).ToList();

            await rolePermissionRepository.AddRangeAsync(newRolePermissions);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                NormalizedEmail = normalizedEmail,
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
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, "Pp123456*");
            await userRepository.AddAsync(newUser);

            var newUserRole = new UserRole
            {
                RoleId = existingAdminRole.Id,
                UserId = newUser.Id
            };
            await userRoleRepository.AddAsync(newUserRole);

            await transaction.CommitAsync();
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error occurred while creating admin user");

            throw;
        }
    }
}