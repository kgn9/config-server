using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Contracts.Operations;

public static class GetConfig
{
    public sealed record Request(string Key, string Namespace, string Profile, ConfigEnvironment Environment);

    public abstract record Result
    {
        public sealed record Success(ConfigItem ConfigItem) : Result;

        public sealed record NotFound : Result;
    }
}