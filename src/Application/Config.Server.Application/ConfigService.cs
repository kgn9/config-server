using Config.Server.Application.Abstractions;
using Config.Server.Application.Contracts;
using Config.Server.Application.Models;

namespace Config.Server.Application;

internal class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;

    public ConfigService(IConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task SetConfigAsync(string key, string value, CancellationToken cancellationToken)
    {
        await _configRepository.AddOrUpdateConfigAsync(key, value, cancellationToken);
    }

    public async Task<ConfigItem> GetConfigAsync(string key, CancellationToken cancellationToken)
    {
        ConfigQuery query = new(Keys: [key], PageSize: 1);
        ConfigItem[] configs = await _configRepository.QueryConfigsAsync(query, cancellationToken).ToArrayAsync(cancellationToken);

        return configs.FirstOrDefault() ?? throw new KeyNotFoundException($"Configuration with key '{key}' not found.");
    }

    public IAsyncEnumerable<ConfigItem> QueryConfigsAsync(
        ConfigQuery query,
        CancellationToken cancellationToken)
    {
        return _configRepository.QueryConfigsAsync(query, cancellationToken);
    }

    public async Task<bool> DeleteConfigAsync(string key, CancellationToken cancellationToken)
    {
        return await _configRepository.DeleteConfigAsync(key, cancellationToken);
    }
}