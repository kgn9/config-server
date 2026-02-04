using Config.Server.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Config.Server.Api.Http.Controllers;

[ApiController]
[Route("projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProjectAsync([FromQuery] string name)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } userId)
            return Unauthorized();

        await _projectService.CreateProject(name, Guid.Parse(userId), HttpContext.RequestAborted);

        return Ok();
    }

    [HttpPost("{project}")]
    [Authorize(Policy = "Maintainer")]
    public async Task<IActionResult> SetRoleToMemberAsync(
        [FromRoute] string project,
        [FromQuery] string username,
        [FromQuery] string role)
    {
        await _projectService.SetRoleToMember(project, username, role, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpDelete("{project}")]
    [Authorize(Policy = "Maintainer")]
    public async Task<IActionResult> RevokeRoleFromMemberAsync(
        [FromRoute] string project,
        [FromQuery] string username)
    {
        await _projectService.RevokeRoleFromMemberAsync(project, username, HttpContext.RequestAborted);

        return Ok();
    }
}