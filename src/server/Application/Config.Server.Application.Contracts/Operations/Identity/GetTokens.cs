namespace Config.Server.Application.Contracts.Operations.Identity;

public static class GetTokens
{
    public abstract record Result
    {
        public sealed record Success(string AccessToken, string RefreshToken) : Result;

        public sealed record NotFound : Result;
    }
}