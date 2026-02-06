using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;

namespace Config.Server.Infrastructure.Persistence.Repositories;

internal class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectMemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddOrUpdateMemberWithRoleAsync(ProjectMember member, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        insert into project_members (project_id, user_id, role, is_revoked, created_at)
        values (:project_id, :user_id, :role, :is_revoked, :created_at)
        on conflict on constraint unique_project_member_pair do update
            set role = excluded.role, is_revoked = excluded.is_revoked;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("project_id", member.ProjectId)
            .AddParameter("user_id", member.UserId)
            .AddParameter("role", member.Role, dataTypeName: "project_roles")
            .AddParameter("is_revoked", member.IsDeleted)
            .AddParameter("created_at", member.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // TODO Remove in favor of using QueryAsync
    public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select * from project_members
        where user_id = :user_id and project_id = :project_id;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter(":user_id", userId)
            .AddParameter(":project_id", projectId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new ProjectMember(
                reader.GetGuid("project_id"),
                reader.GetGuid("user_id"),
                reader.GetFieldValue<ProjectRoles>("role"),
                reader.GetDateTime("created_at"));
        }

        return null;
    }

    public async IAsyncEnumerable<ProjectMember> QueryProjectMemberAsync(
        ProjectMemberQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sqlQuery = """
        select * from project_members
        where
            (created_at > :last_date or created_at = :last_date and user_id = :last_member_id)
            and (:project_id is null or project_id = :project_id)
            and (:user_id is null or user_id = :user_id)
            and (:is_revoked is null or is_revoked = :is_revoked)
        order by created_at, user_id
        limit :page_size;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter("last_date", query.LastDate)
            .AddParameter("last_member_id", query.LastMemberId)
            .AddParameter("project_id", query.ProjectId)
            .AddParameter("user_id", query.MemberId)
            .AddParameter("is_revoked", query.IsRevoked)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ProjectMember(
                reader.GetGuid("project_id"),
                reader.GetGuid("user_id"),
                reader.GetFieldValue<ProjectRoles>("role"),
                reader.GetDateTime("created_at"));
        }
    }
}