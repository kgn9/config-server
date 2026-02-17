namespace Config.Server.Application.Abstractions.Identity;

public interface IApiKeyGenerator
{
    string GetApiKey();
}