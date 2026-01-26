using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Application.Services;
using FluentAssertions;
using NSubstitute;
using System.Text.Json;

namespace Config.Server.Tests;

public class ConfigServiceTests
{
    private readonly IConfigRepository _configRepository;
    private readonly IConfigHistoryRepository _configHistoryRepository;
    private readonly ConfigService _configService;

    public ConfigServiceTests()
    {
        _configRepository = NSubstitute.Substitute.For<IConfigRepository>();
        _configHistoryRepository = NSubstitute.Substitute.For<IConfigHistoryRepository>();
        _configService = new ConfigService(_configRepository, _configHistoryRepository);
    }

    [Fact]
    public async Task GetConfigByKeyAsync_ReturnsSuccess_WhenConfigExists()
    {
        // Arrange
        GetConfig.Request request = new("key1", "TestProject1", "test1", ConfigEnvironment.Global);
        ConfigItem expected = new(
            Id: 1,
            "key1",
            "value1",
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCreator1");

        _configRepository
            .QueryConfigsAsync(
                Arg.Is<ConfigQuery>(q => q.Keys.AsEnumerable().Contains("key1")),
                Arg.Any<CancellationToken>())
            .Returns(new[] { expected }.ToAsyncEnumerable());

        // Act
        GetConfig.Result result = await _configService.GetConfigByKeyAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetConfig.Result.Success>()
            .Which.ConfigItem.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetConfigByKeyAsync_ReturnsNotFound_WhenConfigDoesNotExist()
    {
        // Arrange
        GetConfig.Request request = new("key1", "TestProject1", "test1", ConfigEnvironment.Global);

        _configRepository
            .QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<ConfigItem>());

        // Act
        GetConfig.Result result = await _configService.GetConfigByKeyAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetConfig.Result.NotFound>();
    }

    [Fact]
    public async Task SetConfigAsync_CreatesConfigAndHistory_WhenNewConfigAdded()
    {
        // Arrange
        ConfigItem item = new(
            Id: default,
            "key1",
            "value1",
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCreator1");

        _configRepository.
            QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<ConfigItem>());

        _configRepository.AddOrUpdateConfigAsync(item, Arg.Any<CancellationToken>())
            .Returns(item with { Id = 1 });

        // Act
        await _configService.SetConfigAsync(item, CancellationToken.None);

        // Assert
        await _configRepository.Received(1).AddOrUpdateConfigAsync(item, Arg.Any<CancellationToken>());
        await _configHistoryRepository.Received(1).AddRecordAsync(
            Arg.Is<HistoryItem>(h =>
                h.ConfigId == 1 &&
                h.Operation == ConfigHistoryKind.Created &&
                h.OldValue == "none" &&
                h.NewValue == "value1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetConfigAsync_ShouldUpdateConfigConfigAndHistory_WhenExistingConfigUpdated()
    {
        // Arrange
        ConfigItem existingItem = new(
            Id: default,
            "existingKey",
            "oldValue",
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCreator1");
        ConfigItem updatedItem = new(
            Id: default,
            "existingKey",
            "newValue",
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCreator1");

        _configRepository.
            QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem }.ToAsyncEnumerable());

        _configRepository.AddOrUpdateConfigAsync(updatedItem, Arg.Any<CancellationToken>())
            .Returns(updatedItem with { Id = 1 });

        // Act
        await _configService.SetConfigAsync(updatedItem, CancellationToken.None);

        // Assert
        await _configRepository.Received(1).AddOrUpdateConfigAsync(updatedItem, Arg.Any<CancellationToken>());
        await _configHistoryRepository.Received(1).AddRecordAsync(
            Arg.Is<HistoryItem>(h =>
                h.ConfigId == 1 &&
                h.Operation == ConfigHistoryKind.Updated &&
                h.OldValue == "oldValue" &&
                h.NewValue == "newValue"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteConfigAsync_DeletesConfigAndRecordHistory_WhenConfigExists()
    {
        // Arrange
        var request = new DeleteConfig.Request(
            "existingKey",
            "TestProject1",
            "test1",
            ConfigEnvironment.Global,
            "testCreator1");
        ConfigItem existingItem = new(
            Id: 1,
            "existingKey",
            "oldValue",
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCreator1");

        _configRepository
            .QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem }.ToAsyncEnumerable());

        _configRepository.DeleteConfigAsync(
                request.Namespace,
                request.Profile,
                request.Environment,
                request.Key,
                Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        DeleteConfig.Result result = await _configService.DeleteConfigAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<DeleteConfig.Result.Success>();

        await _configRepository.Received(1).DeleteConfigAsync(
            request.Namespace,
            request.Profile,
            request.Environment,
            request.Key,
            Arg.Any<CancellationToken>());

        await _configHistoryRepository.Received(1).AddRecordAsync(
            Arg.Is<HistoryItem>(h =>
                h.ConfigId == 1 &&
                h.Operation == ConfigHistoryKind.Deleted &&
                h.OldValue == "oldValue" &&
                h.NewValue == "none"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteConfigAsync_ReturnsNotFound_WhenConfigDoesNotExist()
    {
        // Arrange
        var request = new DeleteConfig.Request(
            "unknownKey",
            "TestProject1",
            "test1",
            ConfigEnvironment.Global,
            "testCreator1");

        _configRepository
            .QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<ConfigItem>());

        // Act
        DeleteConfig.Result result = await _configService.DeleteConfigAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<DeleteConfig.Result.ConfigNotFound>();

        await _configRepository.DidNotReceive().DeleteConfigAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ConfigEnvironment>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetConfigsBatchAsync_ShouldFlattenJsonAndCallSetConfigForEachProperty()
    {
        // Arrange
        string json = """
        {
            "Db": {
                "Host": "localhost",
                "Port": 5432
            },
        "Tags": ["a", "b"]
        }
        """;
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        var request = new SetConfigsBatch.Request(
            jsonElement,
            "TestProject1",
            "test",
            ConfigEnvironment.Global,
            "testCreator1");

        _configRepository
            .QueryConfigsAsync(Arg.Any<ConfigQuery>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<ConfigItem>());

        _configRepository.AddOrUpdateConfigAsync(Arg.Any<ConfigItem>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<ConfigItem>());

        // Act
        await _configService.SetConfigsBatchAsync(request, CancellationToken.None);

        // Assert
        await _configRepository.Received(4).AddOrUpdateConfigAsync(Arg.Any<ConfigItem>(), Arg.Any<CancellationToken>());

        await _configRepository.Received().AddOrUpdateConfigAsync(
            Arg.Is<ConfigItem>(c => c.Key == "Db:Host" && c.Value == "localhost"), Arg.Any<CancellationToken>());

        await _configRepository.Received().AddOrUpdateConfigAsync(
            Arg.Is<ConfigItem>(c => c.Key == "Db:Port" && c.Value == "5432"), Arg.Any<CancellationToken>());

        await _configRepository.Received().AddOrUpdateConfigAsync(
            Arg.Is<ConfigItem>(c => c.Key == "Tags:0" && c.Value == "a"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryConfigsAsync_ShouldReturnRepositoryResult()
    {
        // Arrange
        var query = new ConfigQuery([], "ns", "prof", ConfigEnvironment.Global, 10);
        ConfigItem[] expectedItems =
        [
            new ConfigItem(
                Id: 1,
                "key1",
                "value1",
                "TestProject1",
                "test1",
                [ConfigEnvironment.Global],
                DateTime.Today,
                DateTime.Today,
                "testCreator1"),

            new ConfigItem(
                Id: 2,
                "key2",
                "value2",
                "TestProject1",
                "test1",
                [ConfigEnvironment.Global],
                DateTime.Today,
                DateTime.Today,
                "testCreator1")
        ];

        _configRepository.QueryConfigsAsync(query, Arg.Any<CancellationToken>())
            .Returns(expectedItems.ToAsyncEnumerable());

        // Act
        IAsyncEnumerable<ConfigItem> result = _configService.QueryConfigsAsync(query, CancellationToken.None);
        List<ConfigItem> list = await result.ToListAsync();

        // Assert
        list.Should().BeEquivalentTo(expectedItems);
    }
}