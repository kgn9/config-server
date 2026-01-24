using Config.Client.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Config.Client;

internal class ConfigServerRefreshBackgroundService : BackgroundService
{
    private readonly ConfigServerConfigurationProvider _provider;
    private readonly IOptions<ProviderOptions> _options;

    public ConfigServerRefreshBackgroundService(ConfigServerConfigurationProvider provider, IOptions<ProviderOptions> options)
    {
        _provider = provider;
        _options = options;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _provider.ReloadAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Value.UpdateInterval);

        do
        {
            await _provider.ReloadAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}