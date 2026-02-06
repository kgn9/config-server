using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Contracts.Operations.Config;

public static class QueryConfigs
{
    public sealed record Request(
        string Project,
        string Profile,
        string Environment,
        int PageSize,
        string? PageToken);

    public abstract record Result
    {
        public sealed record Success(IAsyncEnumerable<ConfigItem> Items, string PageToken) : Result;
    }
}