using Config.Server.Application.Contracts.Operations.Identity;

namespace Config.Server.Application.Contracts.Services;

public interface IIdentityService
{
    Task<CreateIdentity.Result> CreateIdentityAsync(CreateIdentity.Request request, CancellationToken cancellationToken);

    Task ChangePasswordAsync(ChangePassword.Request request, CancellationToken cancellationToken);

    Task<QueryIdentities.Result> QueryIdentitiesAsync(
        QueryIdentities.Request request,
        CancellationToken cancellationToken);

    Task<CheckCredentials.Result> CheckCredentialsAsync(string username, string password, CancellationToken cancellationToken);

    Task<CheckRefreshToken.Result> CheckRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<GetTokens.Result> GetTokensAsync(Guid userId, CancellationToken cancellationToken);
}