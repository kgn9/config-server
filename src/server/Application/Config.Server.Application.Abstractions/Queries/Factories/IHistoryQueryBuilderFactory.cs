using Config.Server.Application.Abstractions.Queries.Builders;

namespace Config.Server.Application.Abstractions.Queries.Factories;

public interface IHistoryQueryBuilderFactory
{
    IHistoryQueryBuilder Create();
}