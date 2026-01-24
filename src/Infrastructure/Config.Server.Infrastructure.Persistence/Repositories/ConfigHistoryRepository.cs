using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;

namespace Config.Server.Infrastructure.Persistence.Repositories;

public class ConfigHistoryRepository : IConfigHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ConfigHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddRecordAsync(HistoryItem record, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into configuration_history (config_id, operation, old_value, new_value, changed_by, changed_at)
        values (:config_id, :operation, :old_value, :new_value, :changed_by, :changed_at);
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("config_id", record.ConfigId)
            .AddParameter("operation", record.Operation, dataTypeName: "config_history_kind")
            .AddParameter("old_value", record.OldValue)
            .AddParameter("new_value", record.NewValue)
            .AddParameter("changed_by", record.ChangedBy)
            .AddParameter("changed_at", record.ChangedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<HistoryItem> QueryRecordsAsync(
        HistoryQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select id, config_id, operation, old_value, new_value, changed_by, changed_at
        from configuration_history
        where
            config_id >= :cursor
            and (cardinality(:config_ids) = 0 or config_id = any(:config_ids))
            and (cardinality(:operations) = 0 or operation = any(:operations))
            and (:changed_by is null or changed_by like :changed_by)
        order by config_id asc
        limit :page_size;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("cursor", query.Cursor)
            .AddParameter("config_ids", query.ConfigIds)
            .AddParameter("operations", query.Operations, dataTypeName: "config_history_kind[]")
            .AddParameter("changed_by", query.ChangedBy)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new HistoryItem(
                reader.GetInt64("id"),
                reader.GetInt64("config_id"),
                reader.GetFieldValue<ConfigHistoryKind>("operation"),
                reader.GetString("old_value"),
                reader.GetString("new_value"),
                reader.GetString("changed_by"),
                reader.GetDateTime("changed_at"));
        }
    }
}
