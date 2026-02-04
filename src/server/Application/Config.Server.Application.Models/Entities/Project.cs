namespace Config.Server.Application.Models.Entities;

public record Project(
    Guid Id,
    string Name,
    Guid OwnerId,
    DateTime CreatedAt);