using Config.Server.Configuration.HttpClient;
using Config.Server.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Config.Server.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigServer(
        this IServiceCollection services,
        IConfigurationBuilder builder,
        Action<ProviderOptions> action)
    {
        var providerOptions = new ProviderOptions();
        action(providerOptions);

        services.AddOptions();

        services
            .AddRefitClient<IConfigApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(providerOptions.Url));

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