namespace Config.Server.Application.Abstractions.Queries.Models;

public record ProjectMemberQuery(
    Guid? ProjectId,
    Guid? MemberId,
    bool? IsRevoked,
    int PageSize,
    DateTime LastDate,
    Guid LastMemberId);