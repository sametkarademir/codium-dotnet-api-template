using Codium.Template.Domain.Shared.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Codium.Template.HttpApi.Host.Logging.Enrichers;

public class HttpContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var ipAddress = httpContext.GetClientIpAddress();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IpAddress", ipAddress));

        var correlationId = httpContext.GetCorrelationId();
        if (correlationId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        }

        var userId = httpContext.User.GetUserId();
        if (userId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }

        var roles = httpContext.User.GetRoles();
        if (roles.Count != 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", roles));
        }
        
        var permissions = httpContext.User.GetPermissions();
        if (permissions.Count != 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserPermissions", permissions));
        }
        
        var snapshotId = httpContext.GetSnapshotId();
        if (snapshotId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SnapshotId", snapshotId));
        }
        
        var sessionId = httpContext.GetSessionId();
        if (sessionId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SessionId", sessionId));
        }

        var path = httpContext.Request.Path;
        if (path != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Path", path));
        }

        var method = httpContext.Request.Method;
        if (!string.IsNullOrWhiteSpace(method))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Method", method));
        }
    }
}