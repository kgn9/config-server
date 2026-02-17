using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IApiKeyRepository
{
    Task AddOrUpdateAsync(ApiKey apiKey, CancellationToken cancellationToken);

    Task<ApiKey> GetByKey(string key, CancellationToken cancellationToken);
}