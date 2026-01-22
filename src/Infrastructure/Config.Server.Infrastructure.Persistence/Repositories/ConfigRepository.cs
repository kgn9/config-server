using Config.Server.Application.Abstractions;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Application.Models.Queries;
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

    public async Task AddOrUpdateConfigAsync(ConfigItem configItem, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into configurations (key, value, namespace, profile, environment, created_at, updated_at, created_by)
        values (:key, :value, :namespace, :profile, :environment, :created_at, :updated_at, :created_by)
        on conflict on constraint unique_config_record do update set value = excluded.value;
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

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<ConfigItem> QueryConfigsAsync(
        ConfigQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection = _dataSource.CreateConnection();

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
        order by key asc
        limit :pageSize;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("cursor", query.Cursor)
            .AddParameter("pageSize", query.PageSize)
            .AddParameter("keys", query.Keys)
            .AddParameter("namespace", query.Namespace)
            .AddParameter("profile", query.Profile)
            .AddParameter("environment", query.Environment, dataTypeName: "config_environment");

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

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

    public async Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        delete from configurations
        where key = :key;
        """;

        await using NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection)
            .AddParameter("key", key);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows > 0;
    }
}
