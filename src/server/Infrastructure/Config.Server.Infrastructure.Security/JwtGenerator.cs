using Config.Server.Application.Abstractions.Identity;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Config.Server.Infrastructure.Security;

internal class JwtGenerator : IJwtGenerator
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly RsaSecurityKey _securityKey;

    public JwtGenerator(IOptions<JwtOptions> jwtOptions, RsaSecurityKey securityKey)
    {
        _jwtOptions = jwtOptions;
        _securityKey = securityKey;
    }

    public string GetAccessToken(IEnumerable<Claim> claims)
    {
        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Value.Issuer,
            audience: _jwtOptions.Value.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public string GetRefreshToken()
    {
        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Value.Issuer,
            audience: _jwtOptions.Value.Audience,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.RefreshTokenExpirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public bool CheckTokenExpiration(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt = handler.ReadJwtToken(token);

        return jwt.Payload.Expiration < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}