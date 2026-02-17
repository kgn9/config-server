using Config.Server.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Config.Server.Api.Http.Controllers;

[ApiController]
[Route("api/key")]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<string>> GetApiKeyAsync(CancellationToken token)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } userId)
            return Unauthorized();

        string key = await _apiKeyService.GetApiKeyAsync(Guid.Parse(userId), HttpContext.RequestAborted);

        return Ok(key);
    }

    [HttpGet("test")]
    [Authorize(AuthenticationSchemes = "ClientKey")]
    public string Test()
    {
        return "Hello World!";
    }
}