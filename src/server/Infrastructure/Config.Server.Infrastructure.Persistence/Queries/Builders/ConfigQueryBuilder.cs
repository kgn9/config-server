using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Infrastructure.Persistence.Queries.Builders;

public class ConfigQueryBuilder : IConfigQueryBuilder
{
    private string[] _keys = [];
    private string? _namespace;
    private string? _profile;
    private ConfigEnvironment? _environment;
    private int _pageSize = 1;
    private long _cursor = 0;
    private bool _isDeleted = false;

    public IConfigQueryBuilder WithKeys(string[] keys)
    {
        _keys = keys;
        return this;
    }

    public IConfigQueryBuilder WithProject(string project)
    {
        _namespace = project;
        return this;
    }

    public IConfigQueryBuilder WithProfile(string profile)
    {
        _profile = profile;
        return this;
    }

    public IConfigQueryBuilder WithEnvironment(ConfigEnvironment environment)
    {
        _environment = environment;
        return this;
    }

    public IConfigQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public IConfigQueryBuilder WithCursor(long cursor)
    {
        _cursor = cursor;
        return this;
    }

    public IConfigQueryBuilder WithIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    public ConfigQuery Build()
    {
        return new ConfigQuery(
            _keys,
            _namespace,
            _profile,
            _environment,
            _pageSize,
            _cursor,
            _isDeleted);
    }
}