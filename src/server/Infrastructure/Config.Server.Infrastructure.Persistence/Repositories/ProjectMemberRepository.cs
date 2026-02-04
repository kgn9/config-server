using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Extensions;
using Npgsql;
using System.Data;

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
        insert into project_members (project_id, user_id, role)
        values (:project_id, :user_id, :role)
        on conflict on constraint unique_project_member_pair do update
            set role = excluded.role, is_deleted = excluded.is_deleted;
        """;

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sqlQuery;
        command
            .AddParameter(":project_id", member.ProjectId)
            .AddParameter(":user_id", member.UserId)
            .AddParameter(":role", member.Role, dataTypeName: "project_roles")
            .AddParameter("is_deleted", member.IsDeleted);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        const string sqlQuery = """
        select role from project_members
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
                reader.GetFieldValue<ProjectRoles>("role"));
        }

        return null;
    }
}