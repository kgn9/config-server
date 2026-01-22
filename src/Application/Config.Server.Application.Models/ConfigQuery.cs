namespace Config.Server.Application.Models;

public record class ConfigQuery(
    string[]? Keys,
    int PageSize,
    long Cursor = 0);
