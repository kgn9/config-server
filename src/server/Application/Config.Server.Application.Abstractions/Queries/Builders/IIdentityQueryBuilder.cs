using Config.Server.Application.Abstractions.Queries.Models;

namespace Config.Server.Application.Abstractions.Queries.Builders;

public interface IIdentityQueryBuilder
{
    IIdentityQueryBuilder WithIds(Guid[] ids);

    IIdentityQueryBuilder WithUsername(string username);

    IIdentityQueryBuilder WithEmail(string email);

    IIdentityQueryBuilder WithRefreshToken(string refreshToken);

    IIdentityQueryBuilder WithPageSize(int pageSize);

    IIdentityQueryBuilder WithLastDate(DateTime lastDate);

    IIdentityQueryBuilder WithLastId(Guid lastId);

    IdentityQuery Build();
}