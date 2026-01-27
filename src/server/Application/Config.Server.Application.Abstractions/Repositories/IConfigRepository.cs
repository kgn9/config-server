using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IConfigRepository
{
    Task<ConfigItem> AddOrUpdateConfigAsync(ConfigItem configItem, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<long> DeleteConfigAsync(
        string project,
        string profile,
        ConfigEnvironment environment,
        string key,
        CancellationToken cancellationToken);
}