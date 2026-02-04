using Config.Server.Api.Http.Models;
using Config.Server.Api.Http.Options;
using Config.Server.Application.Contracts.Operations.Identity;
using Config.Server.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Config.Server.Api.Http.Controllers;

// TODO Add password change
[ApiController]
[Route("auth")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IOptions<JwtAuthOptions> _jwtAuthOptions;

    public IdentityController(IIdentityService identityService, IOptions<JwtAuthOptions> jwtAuthOptions)
    {
        _identityService = identityService;
        _jwtAuthOptions = jwtAuthOptions;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] IdentityDto dto)
    {
        CreateIdentity.Request request = new(dto.Username, dto.Password, dto.Email);
        CreateIdentity.Result result = await _identityService.CreateIdentityAsync(request, HttpContext.RequestAborted);

        return result switch
        {
            CreateIdentity.Result.Success => Ok(),
            CreateIdentity.Result.IdentityAlreadyExists => Conflict("Username already exists"),
            _ => BadRequest(),
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromQuery] string username,
        [FromQuery] string password)
    {
        CheckCredentials.Result result = await _identityService.CheckCredentialsAsync(username, password, HttpContext.RequestAborted);

        switch (result)
        {
            case CheckCredentials.Result.Success successResult:
            {
                await AddTokensAsync(successResult.Id, HttpContext.RequestAborted);

                return Ok();
            }

            case CheckCredentials.Result.UsernameNotFound:
                return Unauthorized("Username not found");

            case CheckCredentials.Result.PasswordMismatch:
                return Unauthorized("Password mismatch");

            default:
                return Unauthorized();
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync()
    {
        if (!Request.Cookies.TryGetValue(_jwtAuthOptions.Value.RefreshTokenCookieName, out string? refreshToken))
            return Unauthorized();

        CheckRefreshToken.Result result = await _identityService.CheckRefreshTokenAsync(refreshToken, HttpContext.RequestAborted);

        switch (result)
        {
            case CheckRefreshToken.Result.Success successResult:
            {
                await AddTokensAsync(successResult.Id, HttpContext.RequestAborted);

                return Ok();
            }

            case CheckRefreshToken.Result.TokenHasExpired:
                return Unauthorized("Token has expired");

            case CheckRefreshToken.Result.NotFound:
                return Unauthorized("Token not found");

            default:
                return Unauthorized();
        }
    }

    private async Task AddTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        GetTokens.Result tokens = await _identityService.GetTokensAsync(userId, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.Value.AccessTokenExpirationMinutes),
        };

        Response.Cookies.Append(
            _jwtAuthOptions.Value.AccessTokenCookieName,
            tokens.AccessToken,
            cookieOptions);

        cookieOptions.Expires = DateTime.UtcNow.AddHours(_jwtAuthOptions.Value.RefreshTokenExpirationHours);

        Response.Cookies.Append(
            _jwtAuthOptions.Value.RefreshTokenCookieName,
            tokens.RefreshToken,
            cookieOptions);
    }
}