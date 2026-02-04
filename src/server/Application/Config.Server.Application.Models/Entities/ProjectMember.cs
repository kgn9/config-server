using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Models.Entities;

public record ProjectMember(
    Guid ProjectId,
    Guid UserId,
    ProjectRoles Role,
    bool IsDeleted = false);