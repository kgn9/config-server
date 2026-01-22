using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Queries;

namespace Config.Server.Application.Abstractions;

public interface IConfigRepository
{
    Task AddOrUpdateConfigAsync(ConfigItem configItem, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken);
}