using Microsoft.AspNetCore.Builder;

namespace Config.Server.Provider.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMiddlewareRefreshing(this IApplicationBuilder app)
    {
        app.UseMiddleware<ConfigServerRefreshMiddleware>();
        return app;
    }
}