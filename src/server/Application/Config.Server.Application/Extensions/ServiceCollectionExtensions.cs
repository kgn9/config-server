using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Config.Server.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        return services;
    }
}
