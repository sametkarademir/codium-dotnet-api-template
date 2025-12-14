namespace Codium.Template.HttpApi.Host.Middlewares;

public static class ApplicationBuilderExceptionMiddlewareExtensions
{
    public static void UseRegisteredMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<ProgressingStartedMiddleware>();
        app.UseMiddleware<HttpRequestMiddleware>();
    }
}