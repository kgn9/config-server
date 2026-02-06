namespace Config.Server.Application.Abstractions.Queries.Models;

public record ProjectQuery(
    string? Name,
    Guid? OwnerId,
    int PageSize);