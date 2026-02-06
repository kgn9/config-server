using Config.Server.Api.Http.Models;
using Config.Server.Application.Contracts.Operations.Config;
using Config.Server.Application.Contracts.Services;
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
    public async Task<ActionResult<QueryConfigsResponseDto>> QueryConfigsAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromQuery] int pageSize,
        [FromQuery] string? pageToken)
    {
        QueryConfigs.Request request = new(project, profile, environment, pageSize, pageToken);
        QueryConfigs.Result result = await _configService.QueryConfigsAsync(request, HttpContext.RequestAborted);

        if (result is QueryConfigs.Result.Success successResult)
        {
            return Ok(new QueryConfigsResponseDto(
                successResult.Items.Select(x => new ConfigItemResponseDto(x.Key, x.Value)),
                successResult.PageToken));
        }

        return NotFound();
    }

    [HttpGet("{project}/{profile}/{environment}/{key}")]
    [Authorize(Policy = "CanRead")]
    public async Task<ActionResult<ConfigItemResponseDto>> GetConfigByKeyAsync(
        [FromRoute] string project,
        [FromRoute] string profile,
        [FromRoute] string environment,
        [FromRoute] string key)
    {
        GetConfig.Request request = new(key, project, profile, environment);
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

        SetConfig.Request request = new(project, profile, environment, value, key, createdBy);
        await _configService.SetConfigAsync(request, HttpContext.RequestAborted);

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
            environment,
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

        DeleteConfig.Request request = new(key, project, profile, environment, deletedBy);
        DeleteConfig.Result result = await _configService.DeleteConfigAsync(request, HttpContext.RequestAborted);

        return result is DeleteConfig.Result.Success ? Ok() : NotFound();
    }
}
