using Microsoft.Extensions.Configuration;

namespace Config.Server.Provider;

internal class ConfigServerConfigurationSource : IConfigurationSource
{
    private readonly ConfigServerConfigurationProvider _provider;

    public ConfigServerConfigurationSource(ConfigServerConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => _provider;
}