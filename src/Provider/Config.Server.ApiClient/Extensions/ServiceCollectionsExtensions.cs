using Config.Server.ApiClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Config.Server.ApiClient.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClientOptions>(configuration.GetSection("Config"));
        services
            .AddRefitClient<IConfigApiClient>()
            .ConfigureHttpClient((provider,  client) =>
            {
                ClientOptions clientOptions = provider.GetRequiredService<IOptions<ClientOptions>>().Value;

                client.BaseAddress = new Uri(clientOptions.Url);
            });

        return services;
    }
}