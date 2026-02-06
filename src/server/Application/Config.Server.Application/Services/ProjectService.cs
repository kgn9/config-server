using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Services;

internal class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IIdentityQueryBuilderFactory _identityQueryBuilderFactory;
    private readonly IProjectQueryBuilderFactory _projectQueryBuilderFactory;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IIdentityRepository identityRepository,
        IIdentityQueryBuilderFactory identityQueryBuilderFactory,
        IProjectQueryBuilderFactory projectQueryBuilderFactory)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _identityRepository = identityRepository;
        _identityQueryBuilderFactory = identityQueryBuilderFactory;
        _projectQueryBuilderFactory = projectQueryBuilderFactory;
    }

    public async Task CreateProject(string projectName, Guid ownerId, CancellationToken cancellationToken)
    {
        Project project = new(Guid.NewGuid(), projectName, ownerId, DateTime.Now);
        await _projectRepository.CreateProjectAsync(project, cancellationToken);

        ProjectMember member = new(project.Id, ownerId, ProjectRoles.Maintainer, DateTime.UtcNow);
        await _projectMemberRepository.AddOrUpdateMemberWithRoleAsync(member, cancellationToken);
    }

    public async Task SetRoleToMember(
        Guid projectId,
        Guid userId,
        ProjectRoles role,
        CancellationToken cancellationToken)
    {
        ProjectMember member = new(projectId, userId, role, DateTime.UtcNow);
        await _projectMemberRepository.AddOrUpdateMemberWithRoleAsync(member, cancellationToken);
    }

    public async Task SetRoleToMember(
        string projectName,
        string username,
        string role,
        CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder identityQueryBuilder = _identityQueryBuilderFactory.Create();
        IdentityQuery identityQuery = identityQueryBuilder.WithUsername(username).Build();

        UserIdentity? identity = await _identityRepository.QueryIdentitiesAsync(identityQuery, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        IProjectQueryBuilder projectQueryBuilder = _projectQueryBuilderFactory.Create();
        ProjectQuery projectQuery = projectQueryBuilder.WithName(projectName).Build();

        Project? project = await _projectRepository
            .QueryProjectsAsync(projectQuery, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        // TODO Change exception to result or make a custom exception
        if (identity is null || project is null) throw new Exception("User or project not found");

        ProjectMember member = new(project.Id, identity.Id, StringToProjectRolesMapper(role), DateTime.UtcNow);
        await _projectMemberRepository.AddOrUpdateMemberWithRoleAsync(member, cancellationToken);
    }

    public async Task RevokeRoleFromMemberAsync(
        string projectName,
        string username,
        CancellationToken cancellationToken)
    {
        IIdentityQueryBuilder identityQueryBuilder = _identityQueryBuilderFactory.Create();
        IdentityQuery identityQuery = identityQueryBuilder.WithUsername(username).Build();

        UserIdentity? identity = await _identityRepository.QueryIdentitiesAsync(identityQuery, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        IProjectQueryBuilder projectQueryBuilder = _projectQueryBuilderFactory.Create();
        ProjectQuery projectQuery = projectQueryBuilder.WithName(projectName).Build();

        Project? project = await _projectRepository
            .QueryProjectsAsync(projectQuery, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        // TODO Change exception to result or make a custom exception
        if (identity is null || project is null) throw new Exception("User or project not found");

        if (await _projectMemberRepository.GetMemberAsync(project.Id, identity.Id, cancellationToken) is not { } member)
            throw new Exception($"Member {username} not found");

        await _projectMemberRepository.AddOrUpdateMemberWithRoleAsync(member with { IsDeleted = true }, cancellationToken);
    }

    private static ProjectRoles StringToProjectRolesMapper(string role)
    {
        return role switch
        {
            "Maintainer" => ProjectRoles.Maintainer,
            "Editor" => ProjectRoles.Editor,
            _ => ProjectRoles.Reader,
        };
    }
}