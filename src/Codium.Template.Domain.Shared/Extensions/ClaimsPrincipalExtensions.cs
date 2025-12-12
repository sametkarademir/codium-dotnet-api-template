using System.Security.Claims;
using Codium.Template.Domain.Shared.Security;

namespace Codium.Template.Domain.Shared.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from the claims principal as a string.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>The user ID as a string, or null if not found.</returns>
    /// <remarks>
    /// This method retrieves the user ID from the claims principal using the <see cref="ClaimTypes.NameIdentifier"/> claim type.
    /// If the claim is not found, it returns null.
    /// </remarks>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }
        
        return null;
    }

    public static ClaimsIdentity AddUserId(this ClaimsIdentity identity, Guid userId)
    {
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        return identity;
    }

    /// <summary>
    /// Gets the username from the claims principal.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>The username as a string, or null if not found.</returns>
    /// <remarks>
    /// This method retrieves the username from the claims principal using the <see cref="ClaimTypes.Name"/> claim type.
    /// If the claim is not found, it returns null.
    /// </remarks>
    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    public static ClaimsIdentity AddUserName(this ClaimsIdentity identity, string userName)
    {
        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        return identity;
    }

    /// <summary>
    /// Gets the email address from the claims principal.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>The email address as a string, or null if not found.</returns>
    /// <remarks>
    /// This method retrieves the email address from the claims principal using the <see cref="ClaimTypes.Email"/> claim type.
    /// If the claim is not found, it returns null.
    /// </remarks>
    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    public static ClaimsIdentity AddUserEmail(this ClaimsIdentity identity, string email)
    {
        identity.AddClaim(new Claim(ClaimTypes.Name, email));

        return identity;
    }

    /// <summary>
    /// Gets the user roles from the claims principal.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>A list of role names as strings, or null if no roles are found.</returns>
    /// <remarks>
    /// This method retrieves all role claims from the claims principal using the <see cref="ClaimTypes.Role"/> claim type.
    /// If no role claims are found, it returns null.
    /// </remarks>
    public static List<string> GetRoles(this ClaimsPrincipal user)
    {
        var userRolesClaim = user.FindAll(ClaimTypes.Role);
        
        return userRolesClaim.Select(item => item.Value).ToList();
    }
    
    public static ClaimsIdentity AddRoles(this ClaimsIdentity identity, IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return identity;
    }

    /// <summary>
    /// Gets the user permissions from the claims principal.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>A list of permission names as strings.</returns>
    /// <remarks>
    /// This method retrieves all permission claims from the claims principal using the "permission" claim type.
    /// </remarks>
    public static List<string> GetPermissions(this ClaimsPrincipal user)
    {
        var userPermissionsClaim = user.FindAll(CustomClaimTypes.Permission);

        return userPermissionsClaim.Select(item => item.Value).ToList();
    }
    
    public static ClaimsIdentity AddPermissions(this ClaimsIdentity identity, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission));
        }

        return identity;
    }

    /// <summary>
    /// Gets the user session ID from the claims principal.
    /// </summary>
    /// <para>Type="session_id" claim is used to store the session ID.</para>
    /// <param name="user">The claims principal.</param>
    /// <returns>The user session ID as a GUID, or null if not found or invalid.</returns>
    public static Guid? GetSessionId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(CustomClaimTypes.SessionId)?.Value;
        if (Guid.TryParse(value, out var sessionId))
        {
            return sessionId;
        }
        
        return null;
    }
    
    public static ClaimsIdentity AddSessionId(this ClaimsIdentity identity, Guid sessionId)
    {
        identity.AddClaim(new Claim(CustomClaimTypes.SessionId, sessionId.ToString()));
        
        return identity;
    }
    
    public static ClaimsIdentity AddUserClaims(
        this ClaimsIdentity identity, 
        Guid userId, 
        string userName, 
        Guid sessionId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        return identity
            .AddUserId(userId)
            .AddUserName(userName)
            .AddSessionId(sessionId)
            .AddRoles(roles)
            .AddPermissions(permissions);
    }

    /// <summary>
    /// Gets a custom property from the claims principal.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="key">The key of the custom property to retrieve.</param>
    /// <returns>The value of the custom property as a string, or null if not found.</returns>
    /// <remarks>
    /// This method retrieves a custom property from the claims principal using the specified key.
    /// If the claim is not found, it returns null.
    /// </remarks>
    public static string? GetUserCustomProperty(this ClaimsPrincipal user, string key)
    {
        return user.FindFirst(key)?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="role">The role name to check.</param>
    /// <returns>True if the user has the specified role, otherwise false.</returns>
    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    /// <summary>
    /// Checks if the user has any of the specified roles.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="roles">The role names to check.</param>
    /// <returns>True if the user has at least one of the specified roles, otherwise false.</returns>
    public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(user.IsInRole);
    }

    /// <summary>
    /// Checks if the user has all of the specified roles.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="roles">The role names to check.</param>
    /// <returns>True if the user has all of the specified roles, otherwise false.</returns>
    public static bool HasAllRoles(this ClaimsPrincipal user, params string[] roles)
    {
        return roles.All(user.IsInRole);
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="permission">The permission name to check.</param>
    /// <returns>True if the user has the specified permission, otherwise false.</returns>
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        var permissions = user.GetPermissions();
        
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has any of the specified permissions.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="permissions">The permission names to check.</param>
    /// <returns>True if the user has at least one of the specified permissions, otherwise false.</returns>
    public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
    {
        var userPermissions = user.GetPermissions();
        
        return permissions.Any(permission => userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the user has all of the specified permissions.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <param name="permissions">The permission names to check.</param>
    /// <returns>True if the user has all of the specified permissions, otherwise false.</returns>
    public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
    {
        var userPermissions = user.GetPermissions();
       
        return permissions.All(permission => userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    /// <param name="user">The claims principal.</param>
    /// <returns>True if the user is authenticated, otherwise false.</returns>
    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated ?? false;
    }
}