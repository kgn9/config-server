using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Abstractions.Queries.Models;

public record class ConfigQuery(
    string[] Keys,
    string? Namespace,
    string? Profile,
    ConfigEnvironment? Environment,
    int PageSize,
    long LastId,
    bool IsDeleted = false);
