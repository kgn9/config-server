using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Operations.Config;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using Config.Server.Application.Utils;
using System.Text.Json;
using System.Transactions;

namespace Config.Server.Application.Services;

internal class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;
    private readonly IConfigHistoryRepository _configHistoryRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IConfigQueryBuilderFactory _configQueryBuilderFactory;

    public ConfigService(
        IConfigRepository configRepository,
        IConfigHistoryRepository configHistoryRepository,
        IProjectRepository projectRepository,
        IConfigQueryBuilderFactory configQueryBuilderFactory)
    {
        _configRepository = configRepository;
        _configHistoryRepository = configHistoryRepository;
        _projectRepository = projectRepository;
        _configQueryBuilderFactory = configQueryBuilderFactory;
    }

    public async Task SetConfigAsync(SetConfig.Request request, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        ConfigItem configItem = new(
            Id: default,
            request.Key,
            request.Value,
            request.Project,
            request.Profile,
            StringToConfigEnvironment(request.Environment),
            DateTime.UtcNow,
            DateTime.UtcNow,
            request.CreatedBy,
            IsDeleted: false);

        // TODO Replace exception with result or make custom exception
        if (await _projectRepository.GetProjectByNameAsync(configItem.Project, cancellationToken) is null)
            throw new Exception($"Project {configItem.Project} not found");

        GetConfig.Request getRequest = new(
            configItem.Key,
            configItem.Project,
            configItem.Profile,
            configItem.Environment.ToString());
        GetConfig.Result result = await GetConfigByKeyAsync(getRequest, cancellationToken);

        configItem = await _configRepository.AddOrUpdateConfigAsync(configItem, cancellationToken);

        HistoryItem historyItem = new(
            Id: default,
            configItem.Id,
            Operation: ConfigHistoryKind.Created,
            OldValue: string.Empty,
            configItem.Value,
            configItem.CreatedBy,
            configItem.CreatedAt);

        if (result is GetConfig.Result.Success { ConfigItem.IsDeleted: false } successResult)
        {
            historyItem = historyItem with
            {
                Operation = ConfigHistoryKind.Updated,
                OldValue = successResult.ConfigItem.Value,
            };
        }
        else
        {
            historyItem = historyItem with
            {
                Operation = ConfigHistoryKind.Created,
                OldValue = "none",
            };
        }

        await _configHistoryRepository.AddRecordAsync(historyItem, cancellationToken);

        transaction.Complete();
    }

    public async Task SetConfigsBatchAsync(SetConfigsBatch.Request request, CancellationToken cancellationToken)
    {
        Dictionary<string, string> flattenedJson = new();
        FlattenJson(request.Configs, string.Empty,  flattenedJson);

        foreach (KeyValuePair<string, string> kv in flattenedJson)
        {
            SetConfig.Request setRequest = new(
                request.Project,
                request.Profile,
                request.Environment,
                kv.Value,
                kv.Key,
                request.CreatedBy);
            await SetConfigAsync(setRequest, cancellationToken);
        }
    }

    public async Task<GetConfig.Result> GetConfigByKeyAsync(GetConfig.Request request, CancellationToken cancellationToken)
    {
        IConfigQueryBuilder builder = _configQueryBuilderFactory.Create();
        ConfigQuery query = builder
            .WithKeys([request.Key])
            .WithProject(request.Namespace)
            .WithProfile(request.Profile)
            .WithEnvironment(StringToConfigEnvironment(request.Environment))
            .Build();

        ConfigItem? configItem = await _configRepository
            .QueryConfigsAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        return configItem is not null ? new GetConfig.Result.Success(configItem) : new GetConfig.Result.NotFound();
    }

    public async Task<QueryConfigs.Result> QueryConfigsAsync(
        QueryConfigs.Request request,
        CancellationToken cancellationToken)
    {
        IConfigQueryBuilder builder = _configQueryBuilderFactory.Create();
        ConfigQuery query = builder
            .WithProject(request.Project)
            .WithProfile(request.Profile)
            .WithEnvironment(StringToConfigEnvironment(request.Environment))
            .WithPageSize(request.PageSize)
            .Build();

        if (request.PageToken != null)
        {
            long lastId = PageTokenSerializer.DeserializeToLong(request.PageToken);
            query = query with { LastId = lastId };
        }

        IAsyncEnumerable<ConfigItem> items = _configRepository.QueryConfigsAsync(query, cancellationToken);

        ConfigItem lastItem = await items.LastAsync(cancellationToken);
        string newPageToken = PageTokenSerializer.SerializeFromLong(lastItem.Id);

        return new QueryConfigs.Result.Success(items, newPageToken);
    }

    public async Task<DeleteConfig.Result> DeleteConfigAsync(DeleteConfig.Request request, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        GetConfig.Request searchRequest = new(request.Key, request.Namespace, request.Profile, request.Environment);
        GetConfig.Result searchResult = await GetConfigByKeyAsync(searchRequest, cancellationToken);

        switch (searchResult)
        {
            case GetConfig.Result.Success successResult:
            {
                await _configRepository.AddOrUpdateConfigAsync(
                    successResult.ConfigItem with { IsDeleted = true },
                    cancellationToken);

                HistoryItem historyItem = new(
                    Id: default,
                    successResult.ConfigItem.Id,
                    ConfigHistoryKind.Deleted,
                    successResult.ConfigItem.Value,
                    "none",
                    request.DeletedBy,
                    DateTime.Now);

                await _configHistoryRepository.AddRecordAsync(historyItem, cancellationToken);

                transaction.Complete();

                return new DeleteConfig.Result.Success();
            }

            case GetConfig.Result.NotFound:
                transaction.Complete();
                return new DeleteConfig.Result.ConfigNotFound();

            default:
                transaction.Complete();
                return new DeleteConfig.Result.Failure();
        }
    }

    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string propName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                    FlattenJson(property.Value, propName, result);
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                    FlattenJson(item, $"{prefix}:{index++}", result);

                break;

            default:
                result[prefix] = element.ToString();
                break;
        }
    }

    private ConfigEnvironment StringToConfigEnvironment(string input)
    {
        return input switch
        {
            "dev" => ConfigEnvironment.Dev,
            "stage" => ConfigEnvironment.Stage,
            "prod" => ConfigEnvironment.Prod,
            _ => ConfigEnvironment.Global,
        };
    }
}