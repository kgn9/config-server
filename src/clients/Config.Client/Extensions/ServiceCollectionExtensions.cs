using Config.Client.Http;
using Config.Client.Http.Extensions;
using Config.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Config.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigServer(
        this IServiceCollection services,
        IConfigurationBuilder builder,
        Action<ProviderOptions> action)
    {
        var providerOptions = new ProviderOptions();
        action(providerOptions);

        services.AddApiClient(new Uri(providerOptions.Url));
        services.AddOptions();
        services.AddSingleton(provider =>
        {
            ConfigServerConfigurationProvider configProvider = new(
                provider.GetRequiredService<IConfigApiClient>(),
                providerOptions);

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