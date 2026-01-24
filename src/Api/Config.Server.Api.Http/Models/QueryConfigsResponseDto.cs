namespace Config.Server.Api.Http.Models;

public record QueryConfigsResponseDto(IAsyncEnumerable<ConfigItemResponseDto> Items);