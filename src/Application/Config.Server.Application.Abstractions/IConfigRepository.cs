using Config.Server.Application.Models;

namespace Config.Server.Application.Abstractions;

public interface IConfigRepository
{
    Task AddOrUpdateConfigAsync(string key, string value, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);

    Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken);
}