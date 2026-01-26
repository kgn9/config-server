using Config.Server.Api.Http.Models;
using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    public QueryConfigsResponseDto QueryConfigsAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromQuery] int pageSize = 50,
        [FromQuery] int cursor = 0)
    {
        ConfigEnvironment? env = StringToConfigEnvironment(environment);
        ConfigQuery query = new([], project, profile, env, pageSize, cursor);
        IAsyncEnumerable<ConfigItem> config = _configService.QueryConfigsAsync(query, HttpContext.RequestAborted);

        IAsyncEnumerable<ConfigItemResponseDto> dtos = config.Select(x => new ConfigItemResponseDto(x.Key, x.Value));

        return new QueryConfigsResponseDto(dtos);
    }

    [HttpGet("{project}/{profile}/{environment}/{key}")]
    public async Task<ActionResult<ConfigItemResponseDto>> GetConfigByKeyAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key)
    {
        GetConfig.Request request = new(key, project, profile, StringToConfigEnvironment(environment));
        GetConfig.Result result = await _configService.GetConfigByKeyAsync(request, HttpContext.RequestAborted);

        if (result is GetConfig.Result.Success successResult)
        {
            return Ok(
                new ConfigItemResponseDto(
                    successResult.ConfigItem.Key,
                    successResult.ConfigItem.Value));
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost("{project}/{profile}/{environment}/{key}")]
    public async Task<IActionResult> SetConfigByKey(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key,
        [FromQuery] string value,
        [FromQuery] string createdBy)
    {
        ConfigItem item = new(
            Id: default,
            key,
            value,
            project,
            profile,
            [StringToConfigEnvironment(environment)],
            DateTime.Now,
            DateTime.Now,
            createdBy);
        await _configService.SetConfigAsync(item, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpPost("{project}/{profile}/{environment}")]
    public async Task<IActionResult> SetConfigBatch(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromQuery] string creator,
        [FromBody] JsonElement configs)
    {
        SetConfigsBatch.Request request = new(
            configs,
            project,
            profile,
            StringToConfigEnvironment(environment),
            creator);
        await _configService.SetConfigsBatchAsync(request, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpDelete("{project}/{profile}/{environment}/{key}")]
    public async Task<IActionResult> DeleteConfigAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key,
        [FromQuery] string deletedBy)
    {
        DeleteConfig.Request request = new(key, project, profile, StringToConfigEnvironment(environment), deletedBy);
        DeleteConfig.Result result = await _configService.DeleteConfigAsync(request, HttpContext.RequestAborted);

        return result is DeleteConfig.Result.Success successResult ? Ok() : NotFound();
    }

    private ConfigEnvironment StringToConfigEnvironment(string input)
    {
        return input switch
        {
            "dev" => ConfigEnvironment.Dev,
            "stage" => ConfigEnvironment.Stage,
            "prod" => ConfigEnvironment.Prod,
            _ => ConfigEnvironment.Global,
        };
    }
}
