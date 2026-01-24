using Config.Server.ApiClient;
using Config.Server.ApiClient.Models;
using Config.Server.Provider.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Config.Server.Provider;

internal class ConfigServerConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigApiClient _client;
    private readonly IOptions<ProviderOptions> _options;

    internal ConfigServerConfigurationProvider(IConfigApiClient client, IOptions<ProviderOptions> options)
    {
        _options = options;
        _client = client;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        ProviderOptions providerOptions = _options.Value;
        ConfigurationsPage page = await _client.GetConfigurationsAsync(
            providerOptions.Project,
            providerOptions.Profile,
            providerOptions.Environment,
            providerOptions.PageSize,
            providerOptions.Cursor,
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