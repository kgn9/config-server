using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IConfigHistoryRepository
{
    Task AddRecordAsync(HistoryItem record, CancellationToken cancellationToken);

    IAsyncEnumerable<HistoryItem> QueryRecordsAsync(HistoryQuery query, CancellationToken cancellationToken);
}
