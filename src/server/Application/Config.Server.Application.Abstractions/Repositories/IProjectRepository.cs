using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Entities;

namespace Config.Server.Application.Abstractions.Repositories;

public interface IProjectRepository
{
    Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken);

    IAsyncEnumerable<Project> QueryProjectsAsync(ProjectQuery query, CancellationToken cancellationToken);
}