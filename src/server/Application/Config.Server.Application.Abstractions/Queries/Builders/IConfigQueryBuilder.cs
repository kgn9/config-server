using Config.Server.Application.Abstractions.Queries.Models;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Application.Abstractions.Queries.Builders;

public interface IConfigQueryBuilder
{
    IConfigQueryBuilder WithKeys(string[] keys);

    IConfigQueryBuilder WithProject(string project);

    IConfigQueryBuilder WithProfile(string profile);

    IConfigQueryBuilder WithEnvironment(ConfigEnvironment environment);

    IConfigQueryBuilder WithPageSize(int pageSize);

    IConfigQueryBuilder WithCursor(long cursor);

    IConfigQueryBuilder WithIsDeleted(bool isDeleted);

    ConfigQuery Build();
}