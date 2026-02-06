using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Runtime.CompilerServices;

namespace Config.Server.Infrastructure.Persistence.Repositories;

internal class IdentityRepository : IIdentityRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public IdentityRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<UserIdentity> AddOrUpdateIdentityAsync(UserIdentity identity, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into identities (id, username, password, email, refresh_token, created_at)
        values (:id, :username, :password, :email, :refresh_token, :created_at)
        on conflict (id) do update set
            username = excluded.username,
            password = excluded.password,
            email = excluded.email,
            refresh_token = excluded.refresh_token
        returning id;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("id", identity.Id)
            .AddParameter("username", identity.Username)
            .AddParameter("password", identity.Password)
            .AddParameter("email", identity.Email)
            .AddParameter("refresh_token", identity.RefreshToken)
            .AddParameter("created_at", identity.CreatedAt);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return identity with { Id = reader.GetGuid("id") };
    }

    public async IAsyncEnumerable<UserIdentity> QueryIdentitiesAsync(
        IdentityQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select * from identities
        where
            (created_at > :last_date or created_at = :last_date and id > :last_id)
            and (cardinality(:ids) = 0 or id = any(:ids))
            and (:username is null or username = :username)
            and (:email is null or email = :email)
            and (:refresh_token is null or refresh_token = :refresh_token)
        order by created_at, id
        limit :page_size;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("last_date", query.LastDate)
            .AddParameter("last_id", query.LastId)
            .AddParameter("ids", query.Ids)
            .AddParameter("username", query.Username, NpgsqlDbType.Text)
            .AddParameter("email", query.Email, NpgsqlDbType.Text)
            .AddParameter("refresh_token", query.RefreshToken, NpgsqlDbType.Text)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new UserIdentity(
                reader.GetGuid("id"),
                reader.GetString("username"),
                reader.GetString("password"),
                reader.GetString("email"),
                reader.GetNullableString("refresh_token"),
                reader.GetDateTime("created_at"));
        }
    }
}