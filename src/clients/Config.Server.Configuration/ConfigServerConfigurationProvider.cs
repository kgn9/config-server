using Config.Server.Configuration.HttpClient;
using Config.Server.Configuration.Models;
using Config.Server.Configuration.Options;
using Microsoft.Extensions.Configuration;

namespace Config.Server.Configuration;

internal class ConfigServerConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigApiClient _client;
    private readonly ProviderOptions _options;

    internal ConfigServerConfigurationProvider(IConfigApiClient client, ProviderOptions options)
    {
        _options = options;
        _client = client;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        ConfigurationsPage page = await _client.GetConfigurationsAsync(
            _options.Project,
            _options.Profile,
            _options.Environment,
            _options.PageSize,
            _options.Cursor,
            cancellationToken);

        bool reloadFlag = false;
        foreach (ConfigurationItem item in page.Items)
        {
            if (!Data.ContainsKey(item.Key))
            {
                Data.Add(item.Key, item.Value);
                reloadFlag = true;
            }
            else if (Data[item.Key] is null
                || (Data[item.Key] is string existingValue
                    && !existingValue.Equals(item.Value, StringComparison.Ordinal)))
            {
                Data[item.Key] = item.Value;
                reloadFlag = true;
            }
        }

        if (reloadFlag)
            OnReload();
    }
}