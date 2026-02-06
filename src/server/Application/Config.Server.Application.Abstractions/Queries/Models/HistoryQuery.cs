using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Abstractions.Queries.Models;

public record class HistoryQuery(
    long[] ConfigIds,
    ConfigHistoryKind[] Operations,
    string? ChangedBy,
    int PageSize,
    int Cursor);
