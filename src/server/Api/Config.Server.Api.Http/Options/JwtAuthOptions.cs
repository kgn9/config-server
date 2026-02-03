namespace Config.Server.Api.Http.Options;

public class JwtAuthOptions
{
    public string AccessTokenCookieName { get; set; } = "access_token";

    public string RefreshTokenCookieName { get; set; } = "refresh_token";

    public string Authority { get; set; } = "http://localhost:5150";

    public string Issuer { get; set; } = "config-server";

    public string Audience { get; set; } = "audience";

    public int AccessTokenExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationHours { get; set; } = 48;
}