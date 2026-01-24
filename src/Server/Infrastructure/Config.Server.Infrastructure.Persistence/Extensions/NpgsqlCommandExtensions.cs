using Npgsql;
using NpgsqlTypes;

namespace Config.Server.Infrastructure.Persistence.Extensions;

public static class NpgsqlCommandExtensions
{
    public static NpgsqlCommand AddParameter(
        this NpgsqlCommand command,
        string name,
        object? value,
        NpgsqlDbType? dbType = null,
        string? dataTypeName = null)
    {
        NpgsqlParameter parameter = command.Parameters.Add(
            new NpgsqlParameter(name, value is null ? DBNull.Value : value));

        if (dbType is not null)
            parameter.NpgsqlDbType = dbType.Value;

        if (!string.IsNullOrEmpty(dataTypeName))
            parameter.DataTypeName = dataTypeName;

        return command;
    }
}
