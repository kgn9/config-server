using Config.Server.Application.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Config.Server.Infrastructure.Security;

public class ProjectRoleRequirement : IAuthorizationRequirement
{
    public ProjectRoles Role { get; }

    public ProjectRoleRequirement(ProjectRoles role)
    {
        Role = role;
    }
}