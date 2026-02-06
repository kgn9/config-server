using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Application.Abstractions.Queries.Builders;

public interface IProjectMemberQueryBuilder
{
    IProjectMemberQueryBuilder WithProjectId(Guid projectId);

    IProjectMemberQueryBuilder WithMemberId(Guid memberId);

    IProjectMemberQueryBuilder WithIsRevoked(bool isRevoked);

    IProjectMemberQueryBuilder WithPageSize(int pageSize);

    IProjectMemberQueryBuilder WithLastDate(DateTime lastDate);

    IProjectMemberQueryBuilder WithLastMemberId(Guid lastMemberId);

    ProjectMemberQuery Build();
}