using Config.Server.Application.Abstractions.Identity;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyGenerator _keyGenerator;
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyService(
        IApiKeyGenerator keyGenerator,
        IApiKeyRepository apiKeyRepository)
    {
        _keyGenerator = keyGenerator;
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<string> GetApiKeyAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        string key = _keyGenerator.GetApiKey();
        ApiKey apiKey = new(key, ownerId);

        await _apiKeyRepository.AddOrUpdateAsync(apiKey, cancellationToken);

        return key;
    }
}