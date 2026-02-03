namespace Config.Server.Application.Abstractions.Queries;

public record IdentityQuery(
    Guid[] Ids,
    string? Username,
    string? Email,
    string? RefreshToken,
    int PageSize,
    DateTime LastDate,
    Guid LastId);