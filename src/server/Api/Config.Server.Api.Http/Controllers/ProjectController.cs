using Config.Server.Api.Http.Models;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
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

    [HttpPost("{project}/members/{username}/role")]
    [Authorize(Policy = "CanAssignRoles")]
    public async Task<IActionResult> SetRoleToMemberAsync(
        [FromRoute] string project,
        [FromRoute] string username,
        [FromQuery] string role)
    {
        await _projectService.SetRoleToMember(project, username, role, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpDelete("{project}/members/{username}/role")]
    [Authorize(Policy = "CanAssignRoles")]
    public async Task<IActionResult> RevokeRoleFromMemberAsync(
        [FromRoute] string project,
        [FromRoute] string username)
    {
        await _projectService.RevokeRoleFromMemberAsync(project, username, HttpContext.RequestAborted);

        return Ok();
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<IAsyncEnumerable<ProjectDto>>> GetUserProjectsAsync([FromRoute] string username)
    {
        IAsyncEnumerable<Project> projects = await _projectService.GetUserProjectsAsync(username, HttpContext.RequestAborted);

        return Ok(projects.Select(x => new ProjectDto(x.Id, x.Name)));
    }
}