using Microsoft.AspNetCore.Builder;

namespace Config.Client.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMiddlewareRefreshing(this IApplicationBuilder app)
    {
        app.UseMiddleware<ConfigServerRefreshMiddleware>();
        return app;
    }
}