using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Infrastructure.Persistence.Queries.Builders;

public class HistoryQueryBuilder : IHistoryQueryBuilder
{
    private long[] _configIds = [];
    private ConfigHistoryKind[] _operations = [];
    private string? _changedBy;
    private int _pageSize = 1;
    private int _cursor = 0;

    public IHistoryQueryBuilder WithConfigIds(long[] configIds)
    {
        _configIds = configIds;
        return this;
    }

    public IHistoryQueryBuilder WithOperations(ConfigHistoryKind[] operations)
    {
        _operations = operations;
        return this;
    }

    public IHistoryQueryBuilder WithChangedBy(string changedBy)
    {
        _changedBy = changedBy;
        return this;
    }

    public IHistoryQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public IHistoryQueryBuilder WithCursor(int cursor)
    {
        _cursor = cursor;
        return this;
    }

    public HistoryQuery Build()
    {
        return new HistoryQuery(
            _configIds,
            _operations,
            _changedBy,
            _pageSize,
            _cursor);
    }
}