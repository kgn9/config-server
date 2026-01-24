using Config.Client.Http;
using Config.Client.Http.Extensions;
using Config.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Config.Client.Extensions;

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