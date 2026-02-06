using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Contracts.Operations.Config;

public static class GetConfig
{
    public sealed record Request(string Key, string Namespace, string Profile, string Environment);

    public abstract record Result
    {
        public sealed record Success(ConfigItem ConfigItem) : Result;

        public sealed record NotFound : Result;
    }
}