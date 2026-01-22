using Config.Server.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Config.Server.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IConfigService, ConfigService>();

        return services;
    }
}
