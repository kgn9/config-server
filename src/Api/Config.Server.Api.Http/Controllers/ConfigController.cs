using Config.Server.Application.Contracts;
using Config.Server.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Config.Server.Api.Http.Controllers;

[Route("configurations")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public IAsyncEnumerable<ConfigItem> GetConfigs([FromQuery] int pageSize = 50, [FromQuery] int cursor = 0)
    {
        ConfigQuery query = new(null, pageSize, cursor);
        return _configService.QueryConfigsAsync(query, HttpContext.RequestAborted);
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<ConfigItem>> GetConfig(string key)
    {
        try
        {
            ConfigItem config = await _configService.GetConfigAsync(key, HttpContext.RequestAborted);
            return Ok(config);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{key}")]
    public async Task<IActionResult> SetConfig(string key, [FromBody] string value)
    {
        await _configService.SetConfigAsync(key, value, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> SetConfigBatch([FromBody] Dictionary<string, string> configs)
    {
        foreach ((string? key, string? value) in configs)
        {
            await _configService.SetConfigAsync(key, value, HttpContext.RequestAborted);
        }

        return NoContent();
    }
}
