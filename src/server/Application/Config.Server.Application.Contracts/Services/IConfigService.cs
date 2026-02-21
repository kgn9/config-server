using Config.Server.Application.Contracts.Operations.Config;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Contracts.Services;

public interface IConfigService
{
    Task SetConfigAsync(SetConfig.Request request, CancellationToken cancellationToken);

    Task SetConfigsBatchAsync(SetConfigsBatch.Request request, CancellationToken cancellationToken);

    Task<GetConfig.Result> GetConfigByKeyAsync(GetConfig.Request request, CancellationToken cancellationToken);

    Task<QueryConfigs.Result> QueryConfigsAsync(QueryConfigs.Request request, CancellationToken cancellationToken);

    Task<DeleteConfig.Result> DeleteConfigAsync(DeleteConfig.Request request, CancellationToken cancellationToken);

    Task<IAsyncEnumerable<HistoryItem>> QueryConfigHistoryAsync(
        string project,
        string environment,
        string profile,
        string? key,
        CancellationToken cancellationToken);
}
