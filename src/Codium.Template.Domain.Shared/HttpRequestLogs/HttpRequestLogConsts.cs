namespace Codium.Template.Domain.Shared.HttpRequestLogs;

public static class HttpRequestLogConsts
{
    public const int HttpMethodMaxLength = 16;
    public const int RequestPathMaxLength = 2048;
    public const int ClientIpMaxLength = 256;
    public const int UserAgentMaxLength = 2048;
    public const int DeviceFamilyMaxLength = 128;
    public const int DeviceModelMaxLength = 128;
    public const int OsFamilyMaxLength = 128;
    public const int OsVersionMaxLength = 128;
    public const int BrowserFamilyMaxLength = 128;
    public const int BrowserVersionMaxLength = 128;
    public const int ControllerNameMaxLength = 1024;
    public const int ActionNameMaxLength = 1024;
    
    public const bool Enabled = true;
    public const string MaskPattern = "***MASKED***";
    
    public static List<string> ExcludedPaths = ["/health", "/metrics", "/favicon.ico"];
    public static List<string> ExcludedHttpMethods = [];
    public static List<string> ExcludedContentTypes = ["application/octet-stream", "application/pdf", "image/", "video/", "audio/"];
    
    public const bool LogRequestBody = false;
    public const int MaxRequestBodyLength = 5000;
    public const bool LogOnlySlowRequests = false;
    public const long SlowRequestThresholdMs = 10;

    public static List<string> RequestBodySensitiveProperties = ["Password", "Token", "Secret", "Key", "Credential", "Ssn", "Credit", "Card"];
    public static List<string> QueryStringSensitiveProperties = ["Password", "Token", "Secret", "ApiKey", "Key"];
    public static List<string> HeaderSensitiveProperties = ["Authorization", "Cookie", "X-Api-Key"];
}