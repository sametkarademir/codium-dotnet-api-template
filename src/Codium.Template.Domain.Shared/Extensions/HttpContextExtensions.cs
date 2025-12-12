using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UAParser;

namespace Codium.Template.Domain.Shared.Extensions;

/// <summary>
/// Provides extension methods for the HttpContext class to simplify common operations.
/// </summary>
public static class HttpContextExtensions
{
    #region Headers

    /// <summary>
    /// Gets a value from the request headers.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The header key.</param>
    /// <returns>The value of the specified header or null if not found.</returns>
    /// <example>
    /// <code>
    /// string contentType = httpContext.GetRequestHeaderValue("Content-Type");
    /// </code>
    /// </example>
    public static string? GetRequestHeaderValue(this HttpContext context, string key)
    {
        return context.Request.Headers.TryGetValue(key, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Converts all request headers to a dictionary.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>A dictionary containing all request headers.</returns>
    /// <example>
    /// <code>
    /// var headers = httpContext.GetRequestHeadersToDictionary();
    /// foreach (var header in headers)
    /// {
    ///     Console.WriteLine($"{header.Key}: {header.Value}");
    /// }
    /// </code>
    /// </example>
    public static Dictionary<string, string> GetRequestHeadersToDictionary(this HttpContext context)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in context.Request.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        return headers;
    }

    /// <summary>
    /// Converts all request headers to a JSON string.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    ///     /// <param name="jsonSerializerOptions">Optional JSON serializer options.</param>
    /// <returns>A JSON string containing all request headers.</returns>
    /// <example>
    /// <code>
    /// string headersJson = httpContext.GetRequestHeadersToJson();
    /// </code>
    /// </example>
    public static string GetRequestHeadersToJson(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(context.GetRequestHeadersToDictionary(), jsonSerializerOptions);
    }

    /// <summary>
    /// Sets a value in the request headers.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <example>
    /// <code>
    /// httpContext.SetRequestHeaderValue("X-Custom-Header", "CustomValue");
    /// </code>
    /// </example>
    public static void SetRequestHeaderValue(this HttpContext context, string key, string value)
    {
        context.Request.Headers.Append(key, value);
    }

    /// <summary>
    /// Converts all response headers to a JSON string.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options.</param>
    /// <returns>A JSON string containing all response headers.</returns>
    /// <example>
    /// <code>
    /// string responseHeadersJson = httpContext.GetResponseHeadersToJson();
    /// </code>
    /// </example>
    public static string GetResponseHeadersToJson(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in context.Response.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        return JsonSerializer.Serialize(headers, jsonSerializerOptions);
    }

    /// <summary>
    /// Sets a value in the response headers.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <example>
    /// <code>
    /// httpContext.SetResponseHeaderValue("X-Custom-Header", "CustomValue");
    /// </code>
    /// </example>
    public static void SetResponseHeaderValue(this HttpContext context, string key, string value)
    {
        context.Response.Headers.Append(key, value);
    }
    
    /// <summary>
    /// Gets the correlation ID from the request headers
    /// </summary>
    /// <remarks>
    /// This method checks the "X-Correlation-ID" header for a correlation ID.
    /// If the header is not present or the value is not a valid GUID, it returns a new GUID.
    /// </remarks>
    /// <param name="httpContext"></param>
    /// <returns>
    /// A nullable GUID representing the correlation ID.
    /// If the header is not present or the value is not a valid GUID, it returns null.
    /// </returns>
    public static Guid? GetCorrelationId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Correlation-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    /// <summary>
    /// Sets the correlation ID in the request headers
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="correlationId"></param>
    /// <remarks>
    /// This method sets the correlation ID in the request headers.
    /// It is used to track the request across different services.
    /// </remarks>
    public static void SetCorrelationId(this HttpContext httpContext, Guid correlationId)
    {
        httpContext.SetRequestHeaderValue("X-Correlation-ID", correlationId.ToString());
        httpContext.SetResponseHeaderValue("X-Correlation-ID", correlationId.ToString());
    }

    /// <summary>
    /// Gets the session ID from the request headers
    /// </summary>
    /// <remarks>
    /// This method checks the "X-Session-ID" header for a session ID.
    /// If the header is not present or the value is not a valid GUID, it returns null.
    /// </remarks>
    /// <param name="httpContext"></param>
    /// <returns>
    /// A nullable GUID representing the session ID.
    /// If the header is not present or the value is not a valid GUID, it returns null.
    /// </returns>
    public static Guid? GetSessionId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Session-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    /// <summary>
    /// Sets the session ID in the request headers
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="sessionId"></param>
    /// <remarks>
    /// This method sets the session ID in the request headers.
    /// It is used to track the session across different services.
    /// </remarks>
    public static void SetSessionId(this HttpContext httpContext, Guid sessionId)
    {
        httpContext.SetRequestHeaderValue("X-Session-ID", sessionId.ToString());
    }

    /// <summary>
    /// Gets the snapshot ID from the request headers
    /// </summary>
    /// <remarks>
    /// This method checks the "X-Snapshot-ID" header for a snapshot ID.
    /// If the header is not present or the value is not a valid GUID, it returns null.
    /// </remarks>
    /// <param name="httpContext"></param>
    /// <returns>
    /// A nullable GUID representing the snapshot ID.
    /// If the header is not present or the value is not a valid GUID, it returns null.
    /// </returns>
    public static Guid? GetSnapshotId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Snapshot-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    /// <summary>
    /// Sets the snapshot ID in the request headers
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="snapshotId"></param>
    /// <remarks>
    /// This method sets the snapshot ID in the request headers.
    /// It is used to track the snapshot across different services.
    /// </remarks>
    public static void SetSnapshotId(this HttpContext httpContext, Guid snapshotId)
    {
        httpContext.SetRequestHeaderValue("X-Snapshot-ID", snapshotId.ToString());
    }

    #endregion

    #region UserAgent

    /// <summary>
    /// Gets the User-Agent string from the request.
    /// </summary>
    /// <param name="httpContext">The HttpContext instance.</param>
    /// <returns>The User-Agent string.</returns>
    /// <example>
    /// <code>
    /// string userAgent = httpContext.GetUserAgent();
    /// </code>
    /// </example>
    public static string GetUserAgent(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["User-Agent"].ToString();
    }

    /// <summary>
    /// Gets detailed device information from the User-Agent.
    /// </summary>
    /// <param name="httpContext">The HttpContext instance.</param>
    /// <returns>A DeviceInfo object containing detailed device information.</returns>
    /// <remarks>
    /// This method uses the UAParser library to parse the User-Agent string.
    /// </remarks>
    /// <example>
    /// <code>
    /// var deviceInfo = httpContext.GetDeviceInfo();
    /// Console.WriteLine($"Device: {deviceInfo.DeviceFamily}, Browser: {deviceInfo.BrowserFamily}");
    /// </code>
    /// </example>
    public static DeviceInfo GetDeviceInfo(this HttpContext httpContext)
    {
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(httpContext.GetUserAgent());
        var deviceFamily = new DeviceInfo
        {
            DeviceFamily = clientInfo.Device.Family,
            DeviceModel = clientInfo.Device.Model,
            OsFamily = clientInfo.OS.Family,
            OsVersion = string.Join(".", new[]
            {
                clientInfo.OS.Major,
                clientInfo.OS.Minor,
                clientInfo.OS.Patch
            }.Where(v => !string.IsNullOrEmpty(v))),
            BrowserFamily = clientInfo.UA.Family,
            BrowserVersion = string.Join(".", new[]
            {
                clientInfo.UA.Major,
                clientInfo.UA.Minor
            }.Where(v => !string.IsNullOrEmpty(v)))
        };
        var type = GetDeviceType(deviceFamily);
        deviceFamily.IsMobile = type.IsMobile;
        deviceFamily.IsDesktop = type.IsDesktop;
        deviceFamily.IsTablet = type.IsTablet;

        return deviceFamily;
    }

    /// <summary>
    /// Gets the client IP address, handling various proxy scenarios.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The client IP address as a string, or "unknown" if not available.</returns>
    /// <remarks>
    /// This method checks various headers commonly set by proxies (X-Forwarded-For, X-Real-IP)
    /// before falling back to the connection's remote IP address.
    /// </remarks>
    /// <example>
    /// <code>
    /// string clientIp = httpContext.GetClientIpAddress();
    /// </code>
    /// </example>
    public static string GetClientIpAddress(this HttpContext context)
    {
        string? ip = null;

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        if (string.IsNullOrEmpty(ip) && context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            ip = realIp.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }

        return ip ?? "unknown";
    }

    private static (bool IsMobile, bool IsDesktop, bool IsTablet) GetDeviceType(DeviceInfo deviceInfo)
    {
        if (IsTablet(deviceInfo))
        {
            return (false, false, true);
        }

        if (IsMobile(deviceInfo))
        {
            return (true, false, false);
        }

        return (false, true, false);
    }

    private static bool IsTablet(DeviceInfo deviceInfo)
    {
        if (!string.IsNullOrEmpty(deviceInfo.DeviceFamily))
        {
            var deviceFamily = deviceInfo.DeviceFamily.ToLower();
            if (
                deviceFamily.Contains("ipad") ||
                deviceFamily.Contains("tablet") ||
                deviceFamily.Contains("kindle")
            )
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.DeviceModel))
        {
            var deviceModel = deviceInfo.DeviceModel.ToLower();
            if (
                deviceModel.Contains("ipad") ||
                deviceModel.Contains("tablet") ||
                deviceModel.Contains("kindle")
            )
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.BrowserFamily))
        {
            var browser = deviceInfo.BrowserFamily.ToLower();
            if (
                browser.Contains("mobile safari") &&
                !string.IsNullOrEmpty(deviceInfo.OsFamily) &&
                deviceInfo.OsFamily.ToLower().Contains("ios")
            )
            {
                if (
                    !string.IsNullOrEmpty(deviceInfo.DeviceModel) &&
                    deviceInfo.DeviceModel.ToLower().Contains("ipad")
                )
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsMobile(DeviceInfo deviceInfo)
    {
        if (!string.IsNullOrEmpty(deviceInfo.DeviceFamily))
        {
            var deviceFamily = deviceInfo.DeviceFamily.ToLower();
            if (
                deviceFamily.Contains("iphone") ||
                deviceFamily.Contains("android") ||
                deviceFamily.Contains("mobile") ||
                deviceFamily.Contains("phone")
            )
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.OsFamily))
        {
            var osFamily = deviceInfo.OsFamily.ToLower();
            if (
                osFamily.Contains("android") ||
                osFamily.Contains("windows phone") ||
                osFamily.Contains("blackberry")
            )
            {
                return true;
            }

            if (
                osFamily.Contains("ios") &&
                (
                    string.IsNullOrEmpty(deviceInfo.DeviceModel) ||
                    !deviceInfo.DeviceModel.ToLower().Contains("ipad")
                )
            )
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.BrowserFamily))
        {
            var browser = deviceInfo.BrowserFamily.ToLower();
            if (browser.Contains("mobile") && !browser.Contains("tablet"))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region RequestPath

    /// <summary>
    /// Gets the base URL of the current request.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The base URL (scheme and host) of the current request.</returns>
    /// <example>
    /// <code>
    /// string baseUrl = httpContext.GetBaseUrl(); // e.g., "https://example.com"
    /// </code>
    /// </example>
    public static string GetBaseUrl(this HttpContext context)
    {
        return $"{context.Request.Scheme}://{context.Request.Host}";
    }

    /// <summary>
    /// Gets the path of the current request.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The path of the current request.</returns>
    /// <example>
    /// <code>
    /// string path = httpContext.GetPath(); // e.g., "/api/products"
    /// </code>
    /// </example>
    public static string GetPath(this HttpContext context)
    {
        return context.Request.Path.ToString();
    }

    /// <summary>
    /// Gets the HTTP method of the current request.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The HTTP method (GET, POST, etc.) of the current request.</returns>
    /// <example>
    /// <code>
    /// string method = httpContext.GetRequestMethod(); // e.g., "GET" or "POST"
    /// </code>
    /// </example>
    public static string GetRequestMethod(this HttpContext context)
    {
        return context.Request.Method;
    }

    /// <summary>
    /// Gets the controller name from the route data.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The controller name, or "unknown" if not available.</returns>
    /// <example>
    /// <code>
    /// string controller = httpContext.GetControllerName();
    /// </code>
    /// </example>
    public static string? GetControllerName(this HttpContext context)
    {
        var routeValues = GetRouteValue(context);
        
        return routeValues.TryGetValue("controller", out var value) ? value?.ToString() : "unknown";
    }

    /// <summary>
    /// Gets the action name from the route data.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The action name, or "unknown" if not available.</returns>
    /// <example>
    /// <code>
    /// string action = httpContext.GetActionName();
    /// </code>
    /// </example>
    public static string? GetActionName(this HttpContext context)
    {
        var routeValues = GetRouteValue(context);
        
        return routeValues.TryGetValue("action", out var value) ? value?.ToString() : "unknown";
    }

    /// <summary>
    /// Gets the route value dictionary from the context.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>A RouteValueDictionary containing the route values.</returns>
    private static RouteValueDictionary GetRouteValue(HttpContext context)
    {
        var routeData = context.GetRouteData();
        var routeValues = routeData.Values;

        return routeValues;
    }

    #endregion

    #region QueryString

    /// <summary>
    /// Gets a value from the query string.
    /// </summary>
    /// <param name="request">The HttpContext instance.</param>
    /// <param name="key">The query string key.</param>
    /// <returns>The value of the specified query string parameter, or null if not found.</returns>
    /// <example>
    /// <code>
    /// string id = httpContext.GetQueryStringValue("id");
    /// </code>
    /// </example>
    public static string? GetQueryStringValue(this HttpContext request, string key)
    {
        return request.Request.Query.TryGetValue(key, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Converts all query string parameters to a dictionary.
    /// </summary>
    /// <param name="request">The HttpContext instance.</param>
    /// <returns>A dictionary containing all query string parameters.</returns>
    /// <example>
    /// <code>
    /// var queryParams = httpContext.GetQueryStringToDictionary();
    /// foreach (var param in queryParams)
    /// {
    ///     Console.WriteLine($"{param.Key}: {param.Value}");
    /// }
    /// </code>
    /// </example>
    public static Dictionary<string, string> GetQueryStringToDictionary(this HttpContext request)
    {
        var query = new Dictionary<string, string>();
        foreach (var queryItem in request.Request.Query)
        {
            query[queryItem.Key] = queryItem.Value.ToString();
        }

        return query;
    }

    /// <summary>
    /// Converts all query string parameters to a JSON string.
    /// </summary>
    /// <param name="request">The HttpContext instance.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options.</param>
    /// <returns>A JSON string containing all query string parameters.</returns>
    /// <example>
    /// <code>
    /// string queryParamsJson = httpContext.GetQueryStringToJson();
    /// </code>
    /// </example>
    public static string GetQueryStringToJson(this HttpContext request, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(request.GetQueryStringToDictionary(), jsonSerializerOptions);
    }

    #endregion

    #region RequestBody

    /// <summary>
    /// Gets the request body as a string.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="maxLength">Maximum length of the request body to read. Default is 1000.</param>
    /// <param name="trancateMessage">Message to append if the body is truncated. Default is null.</param>
    /// <returns>The request body as a string, truncated if it exceeds the maximum length.</returns>
    /// <remarks>
    /// This method properly handles the request body stream by enabling buffering and resetting the position.
    /// </remarks>
    /// <example>
    /// <code>
    /// string body = httpContext.GetRequestBody();
    /// </code>
    /// </example>
    public static async Task<string> GetRequestBodyAsync(this HttpContext context, int maxLength = 1000, string? trancateMessage = null)
    {
        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }
        else
        {
            context.Request.EnableBuffering();
        }

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var originalContent = await reader.ReadToEndAsync();

        if (originalContent.Length > maxLength)
        {
            originalContent = originalContent.Substring(0, maxLength - trancateMessage?.Length ?? 0 ) + trancateMessage;
        }

        context.Request.Body.Position = 0;

        return originalContent;
    }

    /// <summary>
    /// Gets the request body as a JSON string.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options.</param>
    /// <returns>The request body serialized as a JSON string.</returns>
    /// <example>
    /// <code>
    /// string bodyJson = httpContext.GetRequestBodyToJson();
    /// </code>
    /// </example>
    public static async Task<string> GetRequestBodyToJsonAsync(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var body = await context.GetRequestBodyAsync();
        
        return JsonSerializer.Serialize(body, jsonSerializerOptions);
    }

    #endregion

    #region Form Data

    /// <summary>
    /// Gets a value from the form data asynchronously.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The form field key.</param>
    /// <returns>The value of the specified form field, or null if not found.</returns>
    /// <example>
    /// <code>
    /// string name = await httpContext.GetFormValueAsync("name");
    /// </code>
    /// </example>
    public static async Task<object?> GetFormValueAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.TryGetValue(key, out var values) ? values.FirstOrDefault() : null;
    }

    /// <summary>
    /// Converts all form data to a dictionary asynchronously.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>A dictionary containing all form fields.</returns>
    /// <example>
    /// <code>
    /// var formData = await httpContext.GetFormDataToDictionaryAsync();
    /// foreach (var field in formData)
    /// {
    ///     Console.WriteLine($"{field.Key}: {field.Value}");
    /// }
    /// </code>
    /// </example>
    public static async Task<Dictionary<string, object>> GetFormDataToDictionaryAsync(this HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();
        var formData = new Dictionary<string, object>();

        foreach (var item in form)
        {
            formData[item.Key] = item.Value;
        }

        return formData;
    }

    /// <summary>
    /// Gets a file from the form data asynchronously.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The form file key.</param>
    /// <returns>The IFormFile, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var file = await httpContext.GetFormFileAsync("profilePicture");
    /// if (file != null && file.Length > 0)
    /// {
    ///     // Process the file
    /// }
    /// </code>
    /// </example>
    public static async Task<IFormFile?> GetFormFileAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.Files.GetFile(key);
    }

    /// <summary>
    /// Gets multiple files for a single key from the form data asynchronously.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <param name="key">The form file key.</param>
    /// <returns>A list of IFormFile objects.</returns>
    /// <example>
    /// <code>
    /// var files = await httpContext.GetFormFilesAsync("attachments");
    /// foreach (var file in files)
    /// {
    ///     // Process each file
    /// }
    /// </code>
    /// </example>
    public static async Task<List<IFormFile>> GetFormFilesAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.Files.GetFiles(key).ToList();
    }

    #endregion
}

/// <summary>
/// Represents information about the device and operating system of a user.
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// Gets or sets the family of the device.
    /// </summary>
    public string? DeviceFamily { get; set; }

    /// <summary>
    /// Gets or sets the model of the device.
    /// </summary>
    public string? DeviceModel { get; set; }

    /// <summary>
    /// Gets or sets the family of the operating system.
    /// </summary>
    public string? OsFamily { get; set; }

    /// <summary>
    /// Gets or sets the version of the operating system.
    /// </summary>
    public string? OsVersion { get; set; }

    /// <summary>
    /// Gets or sets the family of the browser.
    /// </summary>
    public string? BrowserFamily { get; set; }

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is a mobile device.
    /// </summary>
    public bool IsMobile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is a tablet.
    /// </summary>
    public bool IsTablet { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is a desktop.
    /// </summary>
    public bool IsDesktop { get; set; }
}