using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Queries;

namespace Config.Server.Application.Contracts;

public interface IConfigService
{
    Task SetConfigAsync(ConfigItem configItem, CancellationToken cancellationToken);

    Task<ConfigItem> GetConfigAsync(string key, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken);
}
