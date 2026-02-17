using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Config.Server.Infrastructure.Security.Handlers;

public class ProjectRoleHandler : AuthorizationHandler<ProjectRoleRequirement>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectQueryBuilderFactory _projectQueryBuilderFactory;
    private readonly IProjectMemberQueryBuilderFactory _projectMemberQueryBuilderFactory;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<CachingOptions> _options;

    public ProjectRoleHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IIdentityRepository identityRepository,
        IIdentityQueryBuilderFactory identityQueryBuilderFactory,
        IProjectQueryBuilderFactory projectQueryBuilderFactory,
        IProjectMemberQueryBuilderFactory projectMemberQueryBuilderFactory,
        IHttpContextAccessor httpContext,
        IMemoryCache memoryCache,
        IOptions<CachingOptions> options)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectQueryBuilderFactory = projectQueryBuilderFactory;
        _projectMemberQueryBuilderFactory = projectMemberQueryBuilderFactory;
        _httpContext = httpContext;
        _memoryCache = memoryCache;
        _options = options;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectRoleRequirement requirement)
    {
        Console.WriteLine("Started handling authorization requirement");

        if (!TryGetUserAndProject(context, out Guid userId, out string projectName))
            return;

        string cache = $"auth_{userId}_{projectName}";

        if (_memoryCache.TryGetValue(cache, out ProjectRoles role) && role <= requirement.Role)
        {
            context.Succeed(requirement);
            return;
        }

        if (_httpContext.HttpContext is var httpContext and not null
            && await TryGetMemberFromDb(userId, projectName, httpContext) is var member and not null
            && member.Role <= requirement.Role)
        {
            _memoryCache.Set(cache, member.Role, _options.Value.CacheExpirationTime);
            context.Succeed(requirement);
        }
    }

    private bool TryGetUserAndProject(
        AuthorizationHandlerContext context,
        out Guid userId,
        out string projectName)
    {
        if (!(_httpContext.HttpContext is var httpContext and not null
            && httpContext.GetRouteData().Values.TryGetValue("project", out object? projectNameObj)
            && projectNameObj?.ToString() is var project and not null
            && context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value is var id and not null))
        {
            userId = Guid.Empty;
            projectName = string.Empty;
            return false;
        }

        userId = Guid.Parse(id);
        projectName = project;

        return true;
    }

    private async Task<ProjectMember?> TryGetMemberFromDb(Guid userId, string projectName, HttpContext httpContext)
    {
        IProjectQueryBuilder projectQueryBuilder = _projectQueryBuilderFactory.Create();
        ProjectQuery projectQuery = projectQueryBuilder.WithName(projectName).Build();

        if (await _projectRepository
                .QueryProjectsAsync(projectQuery, httpContext.RequestAborted)
                .FirstOrDefaultAsync() is not { } project)
        {
            return null;
        }

        IProjectMemberQueryBuilder projectMemberQueryBuilder = _projectMemberQueryBuilderFactory.Create();
        ProjectMemberQuery projectMemberQuery = projectMemberQueryBuilder
            .WithProjectId(project.Id)
            .WithMemberId(userId)
            .Build();

        ProjectMember? member = await _projectMemberRepository
            .QueryProjectMemberAsync(projectMemberQuery, httpContext.RequestAborted)
            .FirstOrDefaultAsync(httpContext.RequestAborted);

        return member;
    }
}