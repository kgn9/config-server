namespace Config.Server.Application.Contracts.Operations.Identity;

public static class ChangeRole
{
    public sealed record Request(Guid UserId, string NewRole);

    public abstract record Result
    {
        public sealed record Success : Result;
    }
}