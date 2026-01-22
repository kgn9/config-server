using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Models.Entities;

public record class ConfigItem(
    long Id,
    string Key,
    string Value,
    string Namespace,
    string Profile,
    ConfigEnvironment[] Environment,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    bool IsDeleted = false);
