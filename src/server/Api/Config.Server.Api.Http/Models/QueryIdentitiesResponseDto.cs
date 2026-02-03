using Config.Server.Application.Models.Entities;

namespace Config.Server.Api.Http.Models;

public record QueryIdentitiesResponseDto(IAsyncEnumerable<UserIdentity> Identities, string? PageToken);