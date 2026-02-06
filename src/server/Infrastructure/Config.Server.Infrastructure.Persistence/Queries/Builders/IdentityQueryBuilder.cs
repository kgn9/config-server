using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Infrastructure.Persistence.Queries.Builders;

public class IdentityQueryBuilder : IIdentityQueryBuilder
{
    private Guid[] _ids = [];
    private string? _username;
    private string? _email;
    private string? _refreshToken;
    private int _pageSize = 1;
    private DateTime _lastDate = DateTime.MinValue;
    private Guid _lastId = Guid.Empty;

    public IIdentityQueryBuilder WithIds(Guid[] ids)
    {
        _ids = ids;
        return this;
    }

    public IIdentityQueryBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public IIdentityQueryBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public IIdentityQueryBuilder WithRefreshToken(string refreshToken)
    {
        _refreshToken = refreshToken;
        return this;
    }

    public IIdentityQueryBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public IIdentityQueryBuilder WithLastDate(DateTime lastDate)
    {
        _lastDate = lastDate;
        return this;
    }

    public IIdentityQueryBuilder WithLastId(Guid lastId)
    {
        _lastId = lastId;
        return this;
    }

    public IdentityQuery Build()
    {
        return new IdentityQuery(
            _ids,
            _username,
            _email,
            _refreshToken,
            _pageSize,
            _lastDate,
            _lastId);
    }
}