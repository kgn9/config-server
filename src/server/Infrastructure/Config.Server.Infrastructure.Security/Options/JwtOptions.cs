namespace Config.Server.Infrastructure.Security.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "config-server";

    public string Audience { get; set; } = "audience";

    public string KeyId { get; set; } = "key1";

    public int KeySize { get; set; } = 2048;

    public int AccessTokenExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationHours { get; set; } = 48;
}