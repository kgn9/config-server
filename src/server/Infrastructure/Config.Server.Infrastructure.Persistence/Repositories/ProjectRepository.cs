using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Runtime.CompilerServices;

namespace Config.Server.Infrastructure.Persistence.Repositories;

internal class ProjectRepository : IProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
         insert into projects (id, name, owner_id, created_at)
         values (:id, :name, :owner_id, :created_at)
         returning id;
         """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("id", project.Id)
            .AddParameter("name", project.Name)
            .AddParameter("owner_id", project.OwnerId)
            .AddParameter("created_at", project.CreatedAt);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return project with { Id = reader.GetGuid("id") };
    }

    // TODO Complete implementation
    public async IAsyncEnumerable<Project> QueryProjectsAsync(
        ProjectQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select * from projects
        where
            (:name is null or name = :name)
            and (:owner_id is null or owner_id = :owner_id)
        order by created_at, id
        limit :page_size;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("name", query.Name, NpgsqlDbType.Text)
            .AddParameter("owner_id", query.OwnerId, NpgsqlDbType.Uuid)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Project(
                reader.GetGuid("id"),
                reader.GetString("name"),
                reader.GetGuid("owner_id"),
                reader.GetDateTime("created_at"));
        }
    }
}