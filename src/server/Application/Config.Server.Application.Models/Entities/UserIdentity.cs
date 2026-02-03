namespace Config.Server.Application.Models.Entities;

public record UserIdentity(
    Guid Id,
    string Username,
    string Password,
    string Email,
    string? RefreshToken,
    DateTime CreatedAt);