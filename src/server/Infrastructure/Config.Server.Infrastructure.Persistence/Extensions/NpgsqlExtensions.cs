using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Config.Server.Infrastructure.Persistence.Extensions;

public static class NpgsqlExtensions
{
    public static NpgsqlCommand AddParameter(
        this NpgsqlCommand command,
        string name,
        object? value,
        NpgsqlDbType? dbType = null,
        string? dataTypeName = null)
    {
        NpgsqlParameter parameter = command.Parameters.Add(
            new NpgsqlParameter(name, value ?? DBNull.Value));

        if (dbType is not null)
            parameter.NpgsqlDbType = dbType.Value;

        if (!string.IsNullOrEmpty(dataTypeName))
            parameter.DataTypeName = dataTypeName;

        return command;
    }

    public static string? GetNullableString(this NpgsqlDataReader reader, string columnName)
    {
        return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(columnName);
    }
}
