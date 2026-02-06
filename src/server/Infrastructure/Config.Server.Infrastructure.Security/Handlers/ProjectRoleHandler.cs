using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
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
    private readonly IIdentityQueryBuilderFactory _identityQueryBuilderFactory;
    private readonly IProjectQueryBuilderFactory _projectQueryBuilderFactory;
    private readonly IHttpContextAccessor _httpContext;

    public ProjectRoleHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IIdentityRepository identityRepository,
        IIdentityQueryBuilderFactory identityQueryBuilderFactory,
        IProjectQueryBuilderFactory projectQueryBuilderFactory,
        IHttpContextAccessor httpContext)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _identityRepository = identityRepository;
        _identityQueryBuilderFactory = identityQueryBuilderFactory;
        _projectQueryBuilderFactory = projectQueryBuilderFactory;
        _httpContext = httpContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectRoleRequirement requirement)
    {
        if (context.User.FindFirst(ClaimTypes.Name)?.Value is not { } username)
            return;

        if (_httpContext.HttpContext is not { } httpContext)
            return;

        IIdentityQueryBuilder identityQueryBuilder = _identityQueryBuilderFactory.Create();
        IdentityQuery identityQuery = identityQueryBuilder.WithUsername(username).Build();

        if (await _identityRepository.QueryIdentitiesAsync(identityQuery, httpContext.RequestAborted).FirstOrDefaultAsync(httpContext.RequestAborted) is not { } identity)
            return;

        if (!httpContext.GetRouteData().Values.TryGetValue("project", out object? projectNameObj) || projectNameObj?.ToString() is not { } projectName)
            return;

        IProjectQueryBuilder projectQueryBuilder = _projectQueryBuilderFactory.Create();
        ProjectQuery projectQuery = projectQueryBuilder.WithName(projectName).Build();

        if (await _projectRepository.QueryProjectsAsync(projectQuery, httpContext.RequestAborted).FirstOrDefaultAsync() is not { } project)
            return;

        ProjectMember? member = await _projectMemberRepository
            .GetMemberAsync(project.Id, identity.Id, httpContext.RequestAborted);
        if (member is null) return;

        if (member.Role <= requirement.Role)
            context.Succeed(requirement);
    }
}