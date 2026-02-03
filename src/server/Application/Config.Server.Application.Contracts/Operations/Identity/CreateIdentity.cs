namespace Config.Server.Application.Contracts.Operations.Identity;

public static class CreateIdentity
{
    public sealed record Request(string Username, string Password, string Email);

    public abstract record Result
    {
        public sealed record Success(Guid UserId) : Result;

        public sealed record IdentityAlreadyExists : Result;
    }
}