namespace Config.Server.Application.Contracts.Services;

public interface IApiKeyService
{
    Task<string> GetApiKeyAsync(Guid ownerId, CancellationToken cancellationToken);
}