using Microsoft.AspNetCore.Authentication;

namespace Config.Server.Infrastructure.Security.Options;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string DefaultScheme => "ClientKey";

    public string HeaderName => "x-api-key";
}