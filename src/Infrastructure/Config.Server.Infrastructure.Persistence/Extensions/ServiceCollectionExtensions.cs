using Config.Server.Application.Abstractions;
using Config.Server.Application.Models.Enums;
using Config.Server.Application.Models.Options;
using Config.Server.Infrastructure.Persistence.Migrations;
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

            Console.WriteLine(connection.ConnectionString);

            var builder = new NpgsqlDataSourceBuilder(connection.ConnectionString);
            builder.MapEnum<ConfigEnvironment>(pgName: "config_environment");

            return builder.Build();
        });

        services.AddScoped(provider =>
            provider.GetRequiredService<NpgsqlDataSource>().CreateConnection());

        services.AddScoped<IConfigRepository, ConfigRepository>();

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
