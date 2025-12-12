using System.Security.Claims;
using Codium.Template.Domain.Shared.Security;

namespace Codium.Template.Domain.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
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

    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    public static ClaimsIdentity AddUserName(this ClaimsIdentity identity, string userName)
    {
        identity.AddClaim(new Claim(ClaimTypes.Name, userName));
        
        return identity;
    }

    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    public static ClaimsIdentity AddUserEmail(this ClaimsIdentity identity, string email)
    {
        identity.AddClaim(new Claim(ClaimTypes.Name, email));

        return identity;
    }

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

    public static string? GetUserCustomProperty(this ClaimsPrincipal user, string key)
    {
        return user.FindFirst(key)?.Value;
    }

    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(user.IsInRole);
    }

    public static bool HasAllRoles(this ClaimsPrincipal user, params string[] roles)
    {
        return roles.All(user.IsInRole);
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        var permissions = user.GetPermissions();
        
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
    {
        var userPermissions = user.GetPermissions();
        
        return permissions.Any(permission => userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase));
    }

    public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
    {
        var userPermissions = user.GetPermissions();
        
        return permissions.All(permission => userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase));
    }

    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated ?? false;
    }
}