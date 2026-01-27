using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;

namespace Config.Server.Infrastructure.Persistence.Repositories;

public class ConfigRepository : IConfigRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ConfigRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<ConfigItem> AddOrUpdateConfigAsync(ConfigItem configItem, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into configurations (key, value, namespace, profile, environment, created_at, updated_at, created_by)
        values (:key, :value, :namespace, :profile, :environment, :created_at, :updated_at, :created_by)
        on conflict on constraint unique_config_record do update
            set value = excluded.value, updated_at = excluded.updated_at, is_deleted = excluded.is_deleted
        returning id;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("key", configItem.Key)
            .AddParameter("value", configItem.Value)
            .AddParameter("namespace", configItem.Namespace)
            .AddParameter("profile", configItem.Profile)
            .AddParameter("environment", configItem.Environment)
            .AddParameter("created_at", configItem.CreatedAt)
            .AddParameter("updated_at", configItem.UpdatedAt)
            .AddParameter("created_by", configItem.CreatedBy);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return configItem with { Id = reader.GetInt64("id") };
    }

    public async IAsyncEnumerable<ConfigItem> QueryConfigsAsync(
        ConfigQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select 
            id, key, value, namespace, profile, environment, created_at, updated_at, created_by
        from configurations
        where
            id >= :cursor
            and (cardinality(:keys) = 0 or key = any(:keys))
            and (:namespace is null or namespace like :namespace)
            and (:profile is null or profile like :profile)
            and (:environment is null or :environment = any(environment))
            and is_deleted = :is_deleted
        order by key
        limit :page_size;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize)
            .AddParameter("keys", query.Keys)
            .AddParameter("namespace", query.Namespace)
            .AddParameter("profile", query.Profile)
            .AddParameter("environment", query.Environment, dataTypeName: "config_environment")
            .AddParameter("is_deleted", query.IsDeleted);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ConfigItem(
                reader.GetInt64("id"),
                reader.GetString("key"),
                reader.GetString("value"),
                reader.GetString("namespace"),
                reader.GetString("profile"),
                reader.GetFieldValue<ConfigEnvironment[]>("environment"),
                reader.GetDateTime("created_at"),
                reader.GetDateTime("updated_at"),
                reader.GetString("created_by"));
        }
    }

    public async Task<long> DeleteConfigAsync(
        string project,
        string profile,
        ConfigEnvironment environment,
        string key,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        update configurations
        set is_deleted = true
        where
            key = :key
            and namespace = :namespace
            and profile = :profile
            and :environment = any(environment)
        returning id;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("key", key)
            .AddParameter("namespace", project)
            .AddParameter("profile", profile)
            .AddParameter("environment", environment, dataTypeName: "config_environment");

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return reader.GetInt64("id");
    }
}
