using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Config.Server.Infrastructure.Security.Handlers;

public class ApiKeyHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IIdentityRepository _identityRepository;
    private readonly IIdentityQueryBuilderFactory _identityQueryBuilderFactory;

    public ApiKeyHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository,
        IMemoryCache memoryCache,
        IIdentityRepository identityRepository,
        IIdentityQueryBuilderFactory identityQueryBuilderFactory)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
        _memoryCache = memoryCache;
        _identityRepository = identityRepository;
        _identityQueryBuilderFactory = identityQueryBuilderFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out StringValues keys)
            || keys.FirstOrDefault() is not { } key)
        {
            return AuthenticateResult.Fail(string.Empty);
        }

        string cache = $"api-key_{key}";
        if (!_memoryCache.TryGetValue(cache, out UserIdentity? owner))
        {
            ApiKey apiKey = await _apiKeyRepository.GetByKey(key, Context.RequestAborted);

            IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
            IdentityQuery query = builder.WithIds([apiKey.OwnerId]).Build();
            owner = await _identityRepository
                .QueryIdentitiesAsync(query, Context.RequestAborted)
                .FirstOrDefaultAsync(Context.RequestAborted);

            _memoryCache.Set(cache, owner);
        }

        if (owner is null)
            return AuthenticateResult.Fail("Owner of key not found");

        IEnumerable<Claim> claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),
            new Claim(ClaimTypes.Name, owner.Username),
        };

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}