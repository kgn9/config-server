using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IProjectMemberRepository
{
    Task AddOrUpdateMemberWithRoleAsync(ProjectMember member, CancellationToken cancellationToken);

    Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
}