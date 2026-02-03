namespace Config.Server.Application.Contracts.Operations.Identity;

public static class CheckRefreshToken
{
    public abstract record Result
    {
        public sealed record Success(Guid Id) : Result;

        public sealed record NotFound : Result;

        public sealed record TokenHasExpired : Result;
    }
}