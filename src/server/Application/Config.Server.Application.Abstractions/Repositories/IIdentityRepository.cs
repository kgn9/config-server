using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

// TODO Add GetByUsername
public interface IIdentityRepository
{
    Task<UserIdentity> AddOrUpdateIdentityAsync(UserIdentity identity, CancellationToken cancellationToken);

    IAsyncEnumerable<UserIdentity> QueryIdentitiesAsync(IdentityQuery query, CancellationToken cancellationToken);
}