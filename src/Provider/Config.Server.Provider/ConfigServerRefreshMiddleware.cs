using Microsoft.AspNetCore.Http;

namespace Config.Server.Provider;

internal class ConfigServerRefreshMiddleware : IMiddleware
{
    private readonly ConfigServerConfigurationProvider _provider;

    public ConfigServerRefreshMiddleware(ConfigServerConfigurationProvider provider)
    {
        _provider = provider;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await _provider.ReloadAsync(context.RequestAborted);
        await next(context);
    }
}