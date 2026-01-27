using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Migrations;
using Config.Server.Infrastructure.Persistence.Options;
using Config.Server.Infrastructure.Persistence.Repositories;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Config.Server.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            IOptions<ConnectionOptions> options = provider.GetRequiredService<IOptions<ConnectionOptions>>();
            ConnectionOptions connection = options.Value;

            var builder = new NpgsqlDataSourceBuilder(connection.ConnectionString);
            builder.MapEnum<ConfigEnvironment>(pgName: "config_environment");
            builder.MapEnum<ConfigHistoryKind>(pgName: "config_history_kind");

            return builder.Build();
        });

        services.AddScoped(provider =>
            provider.GetRequiredService<NpgsqlDataSource>().CreateConnection());

        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IConfigHistoryRepository, ConfigHistoryRepository>();

        return services;
    }

    public static IServiceCollection AddMigrations(
        this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(provider =>
                {
                    IOptions<ConnectionOptions> options = provider.GetRequiredService<IOptions<ConnectionOptions>>();

                    return options.Value.ConnectionString;
                })
                .WithMigrationsIn(typeof(IMigrationAssemblyMarker).Assembly));

        return services;
    }
}
