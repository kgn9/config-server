using Config.Server.Application.Models;

namespace Config.Server.Application.Contracts;

public interface IConfigService
{
    Task SetConfigAsync(string key, string value, CancellationToken cancellationToken);

    Task<ConfigItem> GetConfigAsync(string key, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken);
}
