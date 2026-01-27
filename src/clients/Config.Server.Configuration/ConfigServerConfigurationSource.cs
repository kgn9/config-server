using Microsoft.Extensions.Configuration;

namespace Config.Server.Configuration;

internal class ConfigServerConfigurationSource : IConfigurationSource
{
    private readonly ConfigServerConfigurationProvider _provider;

    public ConfigServerConfigurationSource(ConfigServerConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => _provider;
}