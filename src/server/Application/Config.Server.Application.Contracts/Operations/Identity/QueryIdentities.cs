using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Contracts.Operations.Identity;

public static class QueryIdentities
{
    public sealed record Request(int PageSize, string? PageToken);

    public abstract record Result
    {
        public sealed record Success(IAsyncEnumerable<UserIdentity> Identities, string PageToken) : Result;
    }
}