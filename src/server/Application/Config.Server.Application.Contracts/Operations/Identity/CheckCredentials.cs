namespace Config.Server.Application.Contracts.Operations.Identity;

public static class CheckCredentials
{
    public abstract record Result
    {
        public sealed record Success(Guid Id) : Result;

        public sealed record UsernameNotFound : Result;

        public sealed record PasswordMismatch : Result;
    }
}