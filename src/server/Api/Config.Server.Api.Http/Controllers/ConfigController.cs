using Config.Server.Api.Http.Models;
using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [Authorize(Policy = "CanRead")]
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
    [Authorize(Policy = "CanRead")]
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

        return NotFound();
    }

    [HttpPost("{project}/{profile}/{environment}/{key}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> SetConfigByKey(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key,
        [FromQuery] string value)
    {
        if (User.FindFirst(ClaimTypes.Name)?.Value is not { } createdBy)
            return Unauthorized();

        // TODO Move object creation to service layer
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
    [Authorize(Policy = "CanEdit")]
    public async Task<IActionResult> SetConfigBatch(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromBody] JsonElement configs)
    {
        if (User.FindFirst(ClaimTypes.Name)?.Value is not { } createdBy)
            return Unauthorized();

        SetConfigsBatch.Request request = new(
            configs,
            project,
            profile,
            StringToConfigEnvironment(environment),
            createdBy);
        await _configService.SetConfigsBatchAsync(request, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpDelete("{project}/{profile}/{environment}/{key}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> DeleteConfigAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key)
    {
        if (User.FindFirst(ClaimTypes.Name)?.Value is not { } deletedBy)
            return Unauthorized();

        DeleteConfig.Request request = new(key, project, profile, StringToConfigEnvironment(environment), deletedBy);
        DeleteConfig.Result result = await _configService.DeleteConfigAsync(request, HttpContext.RequestAborted);

        return result is DeleteConfig.Result.Success ? Ok() : NotFound();
    }

    // TODO Move to service layer
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
