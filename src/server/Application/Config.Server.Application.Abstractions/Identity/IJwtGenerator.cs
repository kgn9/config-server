using System.Security.Claims;

namespace Config.Server.Application.Abstractions.Identity;

public interface IJwtGenerator
{
    string GetAccessToken(IEnumerable<Claim> claims);

    string GetRefreshToken();

    bool CheckTokenExpiration(string token);
}