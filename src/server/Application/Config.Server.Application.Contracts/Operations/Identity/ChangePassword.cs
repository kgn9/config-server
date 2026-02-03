namespace Config.Server.Application.Contracts.Operations.Identity;

public static class ChangePassword
{
    public sealed record Request(Guid UserId, string NewPassword);

    public abstract record Result
    {
        public sealed record Success : Result;
    }
}