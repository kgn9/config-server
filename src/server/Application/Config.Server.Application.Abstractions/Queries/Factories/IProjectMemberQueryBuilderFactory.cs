using Config.Server.Application.Abstractions.Queries.Builders;

namespace Config.Server.Application.Abstractions.Queries.Factories;

public interface IProjectMemberQueryBuilderFactory
{
    IProjectMemberQueryBuilder Create();
}