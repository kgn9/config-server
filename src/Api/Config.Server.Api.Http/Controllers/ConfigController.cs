using Config.Server.Api.Http.Models;
using Config.Server.Application.Contracts;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Application.Models.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Config.Server.Api.Http.Controllers;

[Route("configs")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet("{project}/{profile}/{environment}")]
    public IAsyncEnumerable<ConfigItemResponseDto> GetConfigs(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromQuery] int pageSize = 50,
        [FromQuery] int cursor = 0)
    {
        ConfigEnvironment? env = StringToConfigEnvironment(environment);
        ConfigQuery query = new([], project, profile, env, pageSize, cursor);
        IAsyncEnumerable<ConfigItem> config = _configService.QueryConfigsAsync(query, HttpContext.RequestAborted);

        return config.Select(x => new ConfigItemResponseDto(x.Key, x.Value));
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

    [HttpPost]
    public async Task<IActionResult> SetConfig([FromBody] ConfigItemDto config)
    {
        ConfigEnvironment[] envs = config.Environments.Select(x => StringToConfigEnvironment(x)).ToArray();
        ConfigItem configItem = new(
            0,
            config.Key,
            config.Value,
            config.Namespace,
            config.Profile,
            envs,
            DateTime.Now,
            DateTime.Now,
            config.CreatedBy);
        await _configService.SetConfigAsync(configItem, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpPost("{project}/{profile}/{environment}")]
    public async Task<IActionResult> SetConfigBatch(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromQuery] string creator,
        [FromBody] Dictionary<string, string> configs)
    {
        foreach ((string? key, string? value) in configs)
        {
            ConfigItem configItem = new(
                0,
                key,
                value,
                project,
                profile,
                [StringToConfigEnvironment(environment)],
                DateTime.Now,
                DateTime.Now,
                creator);

            await _configService.SetConfigAsync(configItem, HttpContext.RequestAborted);
        }

        return Ok();
    }

    private ConfigEnvironment StringToConfigEnvironment(string input)
    {
        return input switch
        {
            "dev" => ConfigEnvironment.Dev,
            "stage" => ConfigEnvironment.Stage,
            "prod" => ConfigEnvironment.Prod,
            "global" => ConfigEnvironment.Global,
            _ => ConfigEnvironment.Global,
        };
    }
}
