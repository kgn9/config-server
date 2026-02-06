namespace Config.Server.Application.Contracts.Operations.Config;

public static class DeleteConfig
{
    public sealed record Request(
        string Key,
        string Namespace,
        string Profile,
        string Environment,
        string DeletedBy);

    public abstract record Result
    {
        public sealed record Success : Result;

        public sealed record ConfigNotFound : Result;

        public sealed record Failure : Result;
    }
}
