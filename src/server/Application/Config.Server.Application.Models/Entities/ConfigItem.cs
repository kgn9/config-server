using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Models.Entities;

// TODO Remove environments
// TODO Change Id to GUID
// TODO Rename Namespace to Project
public record class ConfigItem(
    long Id,
    string Key,
    string Value,
    string Project,
    string Profile,
    ConfigEnvironment Environment,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    bool IsDeleted = false);
