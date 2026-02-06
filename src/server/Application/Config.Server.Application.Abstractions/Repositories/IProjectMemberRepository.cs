using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IProjectMemberRepository
{
    Task AddOrUpdateMemberWithRoleAsync(ProjectMember member, CancellationToken cancellationToken);

    IAsyncEnumerable<ProjectMember> QueryProjectMemberAsync(
        ProjectMemberQuery query,
        CancellationToken cancellationToken);
}