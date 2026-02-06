using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Infrastructure.Persistence.Queries.Builders;

public class ProjectQueryBuilder : IProjectQueryBuilder
{
    private string? _name;
    private Guid? _ownerId;
    private int _pageSize = 1;

    public IProjectQueryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IProjectQueryBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public IProjectQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public ProjectQuery Build()
    {
        return new ProjectQuery(
            _name,
            _ownerId,
            _pageSize);
    }
}