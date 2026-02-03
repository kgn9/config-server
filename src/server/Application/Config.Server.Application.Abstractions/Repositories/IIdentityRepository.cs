using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IIdentityRepository
{
    Task<UserIdentity> AddOrUpdateIdentityAsync(UserIdentity identity, CancellationToken cancellationToken);

    IAsyncEnumerable<UserIdentity> QueryIdentitiesAsync(IdentityQuery query, CancellationToken cancellationToken);
}