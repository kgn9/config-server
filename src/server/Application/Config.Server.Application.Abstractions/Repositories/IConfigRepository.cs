using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IConfigRepository
{
    Task<ConfigItem> AddOrUpdateConfigAsync(ConfigItem configItem, CancellationToken cancellationToken);

    IAsyncEnumerable<ConfigItem> QueryConfigsAsync(ConfigQuery query, CancellationToken cancellationToken);
}