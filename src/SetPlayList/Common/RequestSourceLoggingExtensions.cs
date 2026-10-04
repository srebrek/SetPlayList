namespace SetPlayList.Common;

internal static class RequestSourceLoggingExtensions
{
    public static IApplicationBuilder UseRequestSourceLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestSourceLoggingMiddleware>();
        return app;
    }
}
