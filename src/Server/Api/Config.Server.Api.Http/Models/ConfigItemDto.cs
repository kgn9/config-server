namespace Config.Server.Api.Http.Models;

public record class ConfigItemDto(
    string Key,
    string Value,
    string Namespace,
    string Profile,
    string[] Environments,
    string CreatedBy);
