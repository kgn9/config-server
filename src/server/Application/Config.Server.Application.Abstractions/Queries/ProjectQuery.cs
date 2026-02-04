namespace Config.Server.Application.Abstractions.Queries;

public record ProjectQuery(
    string? Name,
    Guid? OwnerId,
    int PageSize);