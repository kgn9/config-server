using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Abstractions.Queries.Builders;

public interface IHistoryQueryBuilder
{
    IHistoryQueryBuilder WithConfigIds(long[] configIds);

    IHistoryQueryBuilder WithOperations(ConfigHistoryKind[] operations);

    IHistoryQueryBuilder WithChangedBy(string changedBy);

    IHistoryQueryBuilder WithPageSize(int pageSize);

    IHistoryQueryBuilder WithCursor(int cursor);

    HistoryQuery Build();
}