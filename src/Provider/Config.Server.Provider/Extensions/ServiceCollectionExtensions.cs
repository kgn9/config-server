using Config.Server.ApiClient;
using Config.Server.ApiClient.Extensions;
using Config.Server.Provider.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Config.Server.Provider.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigServer(this IServiceCollection services, IConfigurationBuilder builder)
    {
        services.Configure<ProviderOptions>(builder.Build().GetSection("Config"));
        services.AddApiClient(builder.Build());
        services.AddOptions();
        services.AddSingleton(provider =>
        {
            ConfigServerConfigurationProvider configProvider = new(
                provider.GetRequiredService<IConfigApiClient>(),
                provider.GetRequiredService<IOptions<ProviderOptions>>());

            builder.AddConfigServerConfiguration(configProvider);

            return configProvider;
        });

        services.AddScoped<ConfigServerRefreshMiddleware>();

        return services;
    }

    public static IServiceCollection AddBackgroundConfigRefreshing(this IServiceCollection services)
    {
        services.AddHostedService<ConfigServerRefreshBackgroundService>();

        return services;
    }
}