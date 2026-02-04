using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Config.Server.Infrastructure.Security.Handlers;

public class ProjectRoleHandler : AuthorizationHandler<ProjectRoleRequirement>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IHttpContextAccessor _httpContext;

    public ProjectRoleHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IIdentityRepository identityRepository,
        IHttpContextAccessor httpContext)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _identityRepository = identityRepository;
        _httpContext = httpContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectRoleRequirement requirement)
    {
        if (context.User.FindFirst(ClaimTypes.Name)?.Value is not { } username)
            return;

        var query = new IdentityQuery(
            Ids: [],
            username,
            Email: null,
            RefreshToken: null,
            PageSize: 1,
            LastDate: DateTime.MinValue,
            Guid.Empty);

        if (_httpContext.HttpContext is not { } httpContext)
            return;

        if (await _identityRepository.QueryIdentitiesAsync(query, httpContext.RequestAborted).FirstOrDefaultAsync(httpContext.RequestAborted) is not { } identity)
            return;

        if (!httpContext.GetRouteData().Values.TryGetValue("project", out object? projectNameObj) || projectNameObj?.ToString() is not { } projectName)
            return;

        if (await _projectRepository.GetProjectByNameAsync(projectName, httpContext.RequestAborted) is not { } project)
            return;

        ProjectMember? member = await _projectMemberRepository
            .GetMemberAsync(project.Id, identity.Id, httpContext.RequestAborted);
        if (member is null) return;

        if (member.Role <= requirement.Role)
            context.Succeed(requirement);
    }
}