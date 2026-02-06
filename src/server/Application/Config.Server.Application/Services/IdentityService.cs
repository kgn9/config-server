using Config.Server.Application.Abstractions.Identity;
using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Operations.Identity;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Utils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Transactions;

namespace Config.Server.Application.Services;

internal class IdentityService : IIdentityService
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IIdentityQueryBuilderFactory _identityQueryBuilderFactory;
    private readonly PasswordHasher<object?> _passwordHasher;

    public IdentityService(IIdentityRepository identityRepository, IJwtGenerator jwtGenerator, IIdentityQueryBuilderFactory identityQueryBuilderFactory)
    {
        _identityRepository = identityRepository;
        _jwtGenerator = jwtGenerator;
        _identityQueryBuilderFactory = identityQueryBuilderFactory;
        _passwordHasher = new PasswordHasher<object?>();
    }

    public async Task<CreateIdentity.Result> CreateIdentityAsync(
        CreateIdentity.Request request,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithUsername(request.Username).Build();

        if (await _identityRepository.QueryIdentitiesAsync(query, cancellationToken).AnyAsync(cancellationToken))
            return new CreateIdentity.Result.IdentityAlreadyExists();

        string hashedPassword = _passwordHasher.HashPassword(null, request.Password);

        UserIdentity identity = new(
            Id: Guid.NewGuid(),
            request.Username,
            hashedPassword,
            request.Email,
            RefreshToken: null,
            CreatedAt: DateTime.UtcNow);
        identity = await _identityRepository.AddOrUpdateIdentityAsync(identity, cancellationToken);

        transaction.Complete();

        return new CreateIdentity.Result.Success(identity.Id);
    }

    public async Task<ChangePassword.Result> ChangePasswordAsync(
        ChangePassword.Request request,
        CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithIds([request.UserId]).Build();

        UserIdentity? identity = await _identityRepository
            .QueryIdentitiesAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (identity is null)
            return new ChangePassword.Result.NotFound();

        string hashedPassword = _passwordHasher.HashPassword(null, request.NewPassword);
        await _identityRepository.AddOrUpdateIdentityAsync(identity with { Password = hashedPassword }, cancellationToken);

        return new ChangePassword.Result.Success();
    }

    public async Task<QueryIdentities.Result> QueryIdentitiesAsync(
        QueryIdentities.Request request,
        CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithPageSize(request.PageSize).Build();

        if (request.PageToken != null)
        {
            (DateTime lastDate, Guid lastId) = PageTokenSerializer.Deserialize(request.PageToken);
            query = query with { LastDate = lastDate, LastId = lastId };
        }

        IAsyncEnumerable<UserIdentity> identities = _identityRepository.QueryIdentitiesAsync(query, cancellationToken);

        UserIdentity lastItem = await identities.LastAsync(cancellationToken);
        string newPageToken = PageTokenSerializer.Serialize(lastItem.CreatedAt, lastItem.Id);

        return new QueryIdentities.Result.Success(identities, newPageToken);
    }

    public async Task<CheckCredentials.Result> CheckCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithUsername(username).Build();

        UserIdentity? identity = await _identityRepository
            .QueryIdentitiesAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (identity is null)
            return new CheckCredentials.Result.UsernameNotFound();

        PasswordVerificationResult hashedPassword = _passwordHasher
            .VerifyHashedPassword(null, identity.Password, password);

        return hashedPassword is PasswordVerificationResult.Failed
            ? new CheckCredentials.Result.PasswordMismatch()
            : new CheckCredentials.Result.Success(identity.Id);
    }

    public async Task<CheckRefreshToken.Result> CheckRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithRefreshToken(refreshToken).Build();

        UserIdentity? identity = await _identityRepository
            .QueryIdentitiesAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (identity is null || identity.RefreshToken != refreshToken)
            return new CheckRefreshToken.Result.NotFound();
        if (_jwtGenerator.CheckTokenExpiration(identity.RefreshToken))
            return new CheckRefreshToken.Result.TokenHasExpired();

        return new CheckRefreshToken.Result.Success(identity.Id);
    }

    public async Task<GetTokens.Result> GetTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder builder = _identityQueryBuilderFactory.Create();
        IdentityQuery query = builder.WithIds([userId]).Build();

        UserIdentity? identity = await _identityRepository
            .QueryIdentitiesAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (identity is null)
            return new GetTokens.Result.NotFound();

        IEnumerable<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, identity.Id.ToString()),
            new(ClaimTypes.Name, identity.Username),
            new(ClaimTypes.Email, identity.Email)
        ];

        string accessToken = _jwtGenerator.GetAccessToken(claims);
        string refreshToken = _jwtGenerator.GetRefreshToken();

        await _identityRepository.AddOrUpdateIdentityAsync(identity with { RefreshToken = refreshToken }, cancellationToken);

        return new GetTokens.Result.Success(accessToken, refreshToken);
    }
}