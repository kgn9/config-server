using Config.Server.Application.Abstractions.Identity;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Config.Server.Infrastructure.Security.Generators;

internal class ApiKeyGenerator : IApiKeyGenerator
{
    private readonly IOptions<ApiKeyOptions> _keyOptions;

    public ApiKeyGenerator(IOptions<ApiKeyOptions> keyOptions)
    {
        _keyOptions = keyOptions;
    }

    public string GetApiKey()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(_keyOptions.Value.KeyLength);
        string key = Convert.ToBase64String(bytes);

        return key;
    }
}