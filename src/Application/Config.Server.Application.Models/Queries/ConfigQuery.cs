using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Models.Queries;

public record class ConfigQuery(
    string[] Keys,
    string? Namespace,
    string? Profile,
    ConfigEnvironment? Environment,
    int PageSize,
    long Cursor = 0);
