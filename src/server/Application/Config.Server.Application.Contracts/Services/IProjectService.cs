using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Contracts.Services;

public interface IProjectService
{
    Task CreateProject(string projectName, Guid ownerId, CancellationToken cancellationToken);

    Task SetRoleToMember(
        Guid projectId,
        Guid userId,
        ProjectRoles role,
        CancellationToken cancellationToken);

    Task SetRoleToMember(
        string projectName,
        string username,
        string role,
        CancellationToken cancellationToken);

    Task RevokeRoleFromMemberAsync(
        string projectName,
        string username,
        CancellationToken cancellationToken);
}