#pragma warning disable CS8618

using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Npgsql;
using System.Data;
using Testcontainers.PostgreSql;

namespace Config.Server.Tests;

public class ConfigRepositoryTests : IAsyncLifetime
{
    private const string ConnectionString = "Host=localhost;Port=5433;Database=test-database;Username=postgres;Password=postgres";

    private PostgreSqlContainer _container;
    private NpgsqlDataSource _dataSource;
    private ConfigRepository _configRepository;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder(image: "postgres:latest")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithDatabase("test-database")
            .WithPortBinding(5433, 5432)
            .Build();
        await _container.StartAsync();

        var builder = new NpgsqlDataSourceBuilder(ConnectionString);
        builder.MapEnum<ConfigEnvironment>(pgName: "config_environment");
        _dataSource = builder.Build();

        await InitializeDatabaseSchemaAsync();

        _configRepository = new ConfigRepository(_dataSource);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await _dataSource.DisposeAsync();
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldInsert_WhenConfigIsNew()
    {
        // Arrange
        ConfigItem item = TestHelper.CreateConfigItem("key1", "value1");

        const string sql = """
        select count(*) from configurations
        where key = 'key1';
        """;

        // Act
        ConfigItem result = await _configRepository.AddOrUpdateConfigAsync(item,  CancellationToken.None);

        // Assert
        result.Id.Should().BeGreaterThan(0);

        NpgsqlConnection conn = _dataSource.CreateConnection();
        await conn.OpenAsync();
        await using var command = new NpgsqlCommand(sql, conn);
        long? rowsAffected = (long?)await command.ExecuteScalarAsync();
        rowsAffected.Should().Be(1);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldUpdate_WhenConfigExists()
    {
        // Arrange
        ConfigItem existingItem = TestHelper.CreateConfigItem("existingKey", "existingValue");

        ConfigItem updatedItem = existingItem with { Value = "updatedValue", UpdatedAt = DateTime.Today.AddDays(1) };

        existingItem = await _configRepository.AddOrUpdateConfigAsync(existingItem, CancellationToken.None);

        const string sql = """
        select * from configurations
        where key = 'existingKey';
        """;

        // Act
        updatedItem = await _configRepository.AddOrUpdateConfigAsync(updatedItem, CancellationToken.None);

        // Assert
        updatedItem.Id.Should().Be(existingItem.Id);

        NpgsqlConnection conn = _dataSource.CreateConnection();
        await conn.OpenAsync();
        await using var command = new NpgsqlCommand(sql, conn);
        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        reader.Read().Should().BeTrue();
        reader.GetDateTime("updated_at").Should().Be(updatedItem.UpdatedAt);
        reader.GetString("value").Should().Be(updatedItem.Value);
    }

    [Fact]
    public async Task QueryConfigsAsync_ShouldReturnCorrectConfigs_WhenConfigsArePresent()
    {
        // Arrange
        ConfigItem item1 = TestHelper.CreateConfigItem("key1", "value1");
        ConfigItem item2 = TestHelper.CreateConfigItem("key2", "value2");
        ConfigItem item3 = TestHelper.CreateConfigItem("key3", "value3");

        item1 = await _configRepository.AddOrUpdateConfigAsync(item1, CancellationToken.None);
        item2 = await _configRepository.AddOrUpdateConfigAsync(item2, CancellationToken.None);
        item3 = await _configRepository.AddOrUpdateConfigAsync(item3, CancellationToken.None);
        ConfigItem[] items = [item1, item2, item3];

        ConfigQuery query = new(
            ["key1", "key2", "key3"],
            "TestProject1",
            "test1",
            ConfigEnvironment.Global,
            50,
            0);

        // Act
        IAsyncEnumerable<ConfigItem> result = _configRepository.QueryConfigsAsync(query, CancellationToken.None);

        // Assert
        ConfigItem[] receivedItems = await result.ToArrayAsync();
        receivedItems.Should().BeEquivalentTo(items);
    }

    private async Task InitializeDatabaseSchemaAsync()
    {
        await using var setupDataSource = NpgsqlDataSource.Create(ConnectionString);
        await using NpgsqlConnection connection = await setupDataSource.OpenConnectionAsync();
        await using NpgsqlCommand command = connection.CreateCommand();

        command.CommandText = """
          create type config_environment as enum ('dev', 'stage', 'prod', 'global');
          
          create table configurations
          (
              id          bigint primary key generated always as identity,
          
              key         text,
              value       text not null,
          
              namespace   text not null,
              profile     text not null default 'default',
              environment config_environment[] not null,
          
              created_at  timestamp with time zone not null default now(),
              updated_at  timestamp with time zone not null default now(),
              created_by  text not null,
          
              is_deleted  bool default false,
          
              constraint unique_config_record
                  unique (namespace, profile, environment, key)
          );
          """;

        await command.ExecuteNonQueryAsync();
    }
}
