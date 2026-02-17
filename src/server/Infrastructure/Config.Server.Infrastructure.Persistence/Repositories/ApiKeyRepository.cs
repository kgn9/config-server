using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using System.Data;

namespace Config.Server.Infrastructure.Persistence.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ApiKeyRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddOrUpdateAsync(ApiKey apiKey, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sqlQuery = """
        insert into api_keys (owner_id, key)
        values (:owner_id, :key)
        on conflict (owner_id) do update set key = excluded.key;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("owner_id", apiKey.OwnerId)
            .AddParameter("key", apiKey.Key);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ApiKey> GetByKey(string key, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sqlQuery = """
        select *
        from api_keys
        where key = :key;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command.AddParameter("key", key);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new ApiKey(reader.GetString("key"), reader.GetGuid("owner_id"));
    }
}