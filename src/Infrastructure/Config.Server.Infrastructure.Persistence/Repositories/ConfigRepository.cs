using Config.Server.Application.Abstractions;
using Config.Server.Application.Models;
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

    public async Task AddOrUpdateConfigAsync(string key, string value, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into configurations (key, value)
        values (:key, :value)
        on conflict (key) do update set value = excluded.value;
        """;

        await using NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection)
            .AddParameter("key", key)
            .AddParameter("value", value);

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
        select key, value
        from configurations
        where id >= :cursor
        order by key asc
        limit :pageSize;
        """;

        await using NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("pageSize", query.PageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ConfigItem(
                reader.GetString("key"),
                reader.GetString("value"));
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
