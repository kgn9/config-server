using Config.Server.Application.Abstractions.Queries.Builders;
using Config.Server.Application.Abstractions.Queries.Factories;
using Config.Server.Infrastructure.Persistence.Queries.Builders;

namespace Config.Server.Infrastructure.Persistence.Queries.Factories;

public class IdentityQueryBuilderFactory : IIdentityQueryBuilderFactory
{
    public IIdentityQueryBuilder Create() => new IdentityQueryBuilder();
}