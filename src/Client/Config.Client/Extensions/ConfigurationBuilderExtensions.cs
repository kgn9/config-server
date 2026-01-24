using Microsoft.Extensions.Configuration;

namespace Config.Client.Extensions;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigServerConfiguration(
        this IConfigurationBuilder builder,
        ConfigServerConfigurationProvider provider)
    {
        var source = new ConfigServerConfigurationSource(provider);
        builder.Add(source);

        return builder;
    }
}