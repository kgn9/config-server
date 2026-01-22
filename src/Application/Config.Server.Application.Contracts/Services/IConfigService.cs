using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Contracts.Services;

public interface IConfigService
{
    Task SetConfigAsync(ConfigItem configItem, CancellationToken cancellationToken);

    Task<GetConfig.Result> GetConfigByKeyAsync(GetConfig.Request request, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<DeleteConfig.Result> DeleteConfigAsync(DeleteConfig.Request request, CancellationToken cancellationToken);
}
