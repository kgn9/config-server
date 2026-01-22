using Config.Server.Application.Abstractions.Queries;
using Config.Server.Application.Abstractions.Repositories;
using Config.Server.Application.Contracts.Operations;
using Config.Server.Application.Contracts.Services;
using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;
using System.Transactions;

namespace Config.Server.Application.Services;

internal class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;
    private readonly IConfigHistoryRepository _configHistoryRepository;

    public ConfigService(
        IConfigRepository configRepository,
        IConfigHistoryRepository configHistoryRepository)
    {
        _configRepository = configRepository;
        _configHistoryRepository = configHistoryRepository;
    }

    public async Task SetConfigAsync(ConfigItem configItem, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        GetConfig.Request request = new(configItem.Key, configItem.Namespace, configItem.Profile, configItem.Environment.First());
        GetConfig.Result result = await GetConfigByKeyAsync(request, cancellationToken);

        if (result is GetConfig.Result.Success successResult)
        {
            ConfigItem oldConfigItem = successResult.ConfigItem;

            configItem = await _configRepository.AddOrUpdateConfigAsync(configItem, cancellationToken);

            HistoryItem historyItem = new(
                0,
                configItem.Id,
                ConfigHistoryKind.Updated,
                oldConfigItem.Value,
                configItem.Value,
                configItem.CreatedBy,
                configItem.CreatedAt);

            await _configHistoryRepository.AddRecordAsync(historyItem, cancellationToken);
        }
        else if (result is GetConfig.Result.ConfigNotFound)
        {
            configItem = await _configRepository.AddOrUpdateConfigAsync(configItem, cancellationToken);

            HistoryItem historyItem = new(
                0,
                configItem.Id,
                ConfigHistoryKind.Created,
                "none",
                configItem.Value,
                configItem.CreatedBy,
                configItem.CreatedAt);

            await _configHistoryRepository.AddRecordAsync(historyItem, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task<GetConfig.Result> GetConfigByKeyAsync(GetConfig.Request request, CancellationToken cancellationToken)
    {
        ConfigQuery query = new([request.Key], request.Namespace, request.Profile, request.Environment, PageSize: 1);
        ConfigItem[] configs = await _configRepository.QueryConfigsAsync(query, cancellationToken).ToArrayAsync(cancellationToken);
        ConfigItem? configItem = configs.FirstOrDefault();

        return configItem is not null ? new GetConfig.Result.Success(configItem) : new GetConfig.Result.ConfigNotFound();
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

        long id = await _configRepository.DeleteConfigAsync(
            request.Namespace,
            request.Profile,
            request.Environment,
            request.Key,
            cancellationToken);

        HistoryItem historyItem = new(
            0,
            id,
            ConfigHistoryKind.Deleted,
            string.Empty,
            string.Empty,
            string.Empty,
            DateTime.Now);

        await _configHistoryRepository.AddRecordAsync(historyItem, cancellationToken);

        transaction.Complete();

        return new DeleteConfig.Result.Success();
    }
}