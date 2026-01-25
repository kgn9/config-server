using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Config.Client.Http.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services, Uri url)
    {
        services
            .AddRefitClient<IConfigApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = url);

        return services;
    }
}