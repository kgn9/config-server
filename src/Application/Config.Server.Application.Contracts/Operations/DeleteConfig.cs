using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Contracts.Operations;

public static class DeleteConfig
{
    public sealed record Request(string Key, string Namespace, string Profile, ConfigEnvironment Environment);

    public abstract record Result
    {
        public sealed record Success : Result;

        public sealed record ConfigNotFound : Result;
    }
}
