using Config.Client.Http.Models;
using Refit;

namespace Config.Client.Http;

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