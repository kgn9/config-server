using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Config.Server.Infrastructure.Persistence.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task<IServiceProvider> MigrationsUp(this IServiceProvider provider)
    {
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        return provider;
    }

    public static async Task<IServiceProvider> MigrationsDown(this IServiceProvider provider, int toVersion)
    {
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(toVersion);
        }

        return provider;
    }
}
