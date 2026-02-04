using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using System.Text.Json;
using System.Transactions;

namespace Config.Server.Application.Services;

internal class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;
    private readonly IConfigHistoryRepository _configHistoryRepository;
    private readonly IProjectRepository _projectRepository;

    public ConfigService(
        IConfigRepository configRepository,
        IConfigHistoryRepository configHistoryRepository,
        IProjectRepository projectRepository)
    {
        _configRepository = configRepository;
        _configHistoryRepository = configHistoryRepository;
        _projectRepository = projectRepository;
    }

    public async Task SetConfigAsync(ConfigItem configItem, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // TODO Replace exception with result or make custom exception
        if (await _projectRepository.GetProjectByNameAsync(configItem.Namespace, cancellationToken) is null)
            throw new Exception($"Project {configItem.Namespace} not found");

        GetConfig.Request request = new(configItem.Key, configItem.Namespace, configItem.Profile, configItem.Environment.First());
        GetConfig.Result result = await GetConfigByKeyAsync(request, cancellationToken);

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
            ConfigItem item = new(
                Id: default,
                kv.Key,
                kv.Value,
                request.Project,
                request.Profile,
                [request.Environment],
                DateTime.Now,
                DateTime.Now,
                request.CreatedBy);
            await SetConfigAsync(item, cancellationToken);
        }
    }

    public async Task<GetConfig.Result> GetConfigByKeyAsync(GetConfig.Request request, CancellationToken cancellationToken)
    {
        ConfigQuery query = new([request.Key], request.Namespace, request.Profile, request.Environment, PageSize: 1);
        ConfigItem? configItem = await _configRepository
            .QueryConfigsAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        return configItem is not null ? new GetConfig.Result.Success(configItem) : new GetConfig.Result.NotFound();
    }

    public IAsyncEnumerable<ConfigItem> QueryConfigsAsync(
        ConfigQuery query,
        CancellationToken cancellationToken)
    {
        return _configRepository.QueryConfigsAsync(query, cancellationToken);
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
                long configId = await _configRepository.DeleteConfigAsync(
                    request.Namespace,
                    request.Profile,
                    request.Environment,
                    request.Key,
                    cancellationToken);

                HistoryItem historyItem = new(
                    Id: default,
                    configId,
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
}