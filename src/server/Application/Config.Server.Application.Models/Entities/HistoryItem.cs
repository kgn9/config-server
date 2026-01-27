using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Models.Entities;

public record class HistoryItem(
    long Id,
    long ConfigId,
    ConfigHistoryKind Operation,
    string OldValue,
    string NewValue,
    string ChangedBy,
    DateTime ChangedAt);