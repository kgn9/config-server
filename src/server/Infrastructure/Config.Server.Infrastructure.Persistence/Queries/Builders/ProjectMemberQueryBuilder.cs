using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Infrastructure.Persistence.Queries.Builders;

public class ProjectMemberQueryBuilder : IProjectMemberQueryBuilder
{
    private Guid? _projectId;
    private Guid? _memberId;
    private bool? _isRevoked;
    private int _pageSize = 1;
    private DateTime _lastDate = DateTime.MinValue;
    private Guid _lastMemberId = Guid.Empty;

    public IProjectMemberQueryBuilder WithProjectId(Guid projectId)
    {
        _projectId = projectId;
        return this;
    }

    public IProjectMemberQueryBuilder WithMemberId(Guid memberId)
    {
        _memberId = memberId;
        return this;
    }

    public IProjectMemberQueryBuilder WithIsRevoked(bool isRevoked)
    {
        _isRevoked = isRevoked;
        return this;
    }

    public IProjectMemberQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public IProjectMemberQueryBuilder WithLastDate(DateTime lastDate)
    {
        _lastDate = lastDate;
        return this;
    }

    public IProjectMemberQueryBuilder WithLastMemberId(Guid lastMemberId)
    {
        _lastMemberId = lastMemberId;
        return this;
    }

    public ProjectMemberQuery Build()
    {
        return new ProjectMemberQuery(
            _projectId,
            _memberId,
            _isRevoked,
            _pageSize,
            _lastDate,
            _lastMemberId);
    }
}