using System.Diagnostics;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.HttpRequestLogs;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.HttpApi.Host.Middlewares;

public class HttpRequestMiddleware
{
    private readonly RequestDelegate _next;

    public HttpRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IHttpRequestLogRepository httpRequestLogRepository, IUnitOfWork unitOfWork)
    {
        if (!HttpRequestLogConsts.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.GetPath().ToLowerInvariant();
        if (HttpRequestLogConsts.ExcludedPaths.Any(item => path.StartsWith(item, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (HttpRequestLogConsts.ExcludedHttpMethods.Contains(context.GetRequestMethod(), StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var contentType = context.Request.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (HttpRequestLogConsts.ExcludedContentTypes.Any(ct => contentType.StartsWith(ct, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var executionTime = DateTime.UtcNow;
        string? originalContent = null;
        if (HttpRequestLogConsts.LogRequestBody)
        {
            originalContent = await context.GetRequestBodyAsync(HttpRequestLogConsts.MaxRequestBodyLength);
        }

        await _next(context);
        stopwatch.Stop();

        if (HttpRequestLogConsts.LogOnlySlowRequests && stopwatch.ElapsedMilliseconds < HttpRequestLogConsts.SlowRequestThresholdMs)
        {
            return;
        }

        var deviceInfo = context.GetDeviceInfo();

        var httpRequestLog = new HttpRequestLog
        {
            CreationTime = DateTime.UtcNow,
            CreatorId = context.User.GetUserId(),
            HttpMethod = context.GetRequestMethod(),
            RequestPath = context.GetPath(),
            QueryString = JsonMaskExtensions.MaskSensitiveData(context.GetQueryStringToJson(),
                HttpRequestLogConsts.MaskPattern,
                HttpRequestLogConsts.QueryStringSensitiveProperties.ToArray()),
            RequestBody = JsonMaskExtensions.MaskSensitiveData(originalContent,
                HttpRequestLogConsts.MaskPattern,
                HttpRequestLogConsts.RequestBodySensitiveProperties.ToArray()),
            RequestHeaders = JsonMaskExtensions.MaskSensitiveData(context.GetRequestHeadersToJson(),
                HttpRequestLogConsts.MaskPattern,
                HttpRequestLogConsts.HeaderSensitiveProperties.ToArray()),
            StatusCode = context.Response.StatusCode,
            RequestTime = executionTime,
            ResponseTime = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds,
            ClientIp = context.GetClientIpAddress(),
            UserAgent = context.GetUserAgent(),
            DeviceFamily = deviceInfo.DeviceFamily,
            DeviceModel = deviceInfo.DeviceModel,
            OsFamily = deviceInfo.OsFamily,
            OsVersion = deviceInfo.OsVersion,
            BrowserFamily = deviceInfo.BrowserFamily,
            BrowserVersion = deviceInfo.BrowserVersion,
            IsMobile = deviceInfo.IsMobile,
            IsTablet = deviceInfo.IsTablet,
            IsDesktop = deviceInfo.IsDesktop,
            ControllerName = context.GetControllerName(),
            ActionName = context.GetActionName(),
            SnapshotId = context.GetSnapshotId(),
            SessionId = context.GetSessionId(),
            CorrelationId = context.GetCorrelationId()
        };

        await httpRequestLogRepository.AddAsync(httpRequestLog);
        await unitOfWork.SaveChangesAsync();
    }
}