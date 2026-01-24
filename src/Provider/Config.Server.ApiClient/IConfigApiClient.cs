using Config.Server.ApiClient.Models;
using Refit;

namespace Config.Server.ApiClient;

public interface IConfigApiClient
{
    [Get("/configs/{project}/{profile}/{environment}")]
    Task<ConfigurationsPage> GetConfigurationsAsync(
        string project,
        string profile,
        string environment,
        [Query] int pageSize,
        [Query] int cursor,
        CancellationToken cancellationToken);
}