using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Application.Abstractions.Queries.Builders;

public interface IProjectQueryBuilder
{
    IProjectQueryBuilder WithName(string name);

    IProjectQueryBuilder WithOwnerId(Guid ownerId);

    IProjectQueryBuilder WithPageSize(int pageSize);

    ProjectQuery Build();
}