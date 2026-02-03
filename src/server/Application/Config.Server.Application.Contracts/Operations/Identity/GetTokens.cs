namespace Config.Server.Application.Contracts.Operations.Identity;

public static class GetTokens
{
    public sealed record Result(string AccessToken, string RefreshToken);
}