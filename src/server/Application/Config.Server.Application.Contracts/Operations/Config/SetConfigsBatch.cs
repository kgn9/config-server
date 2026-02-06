using System.Text.Json;

namespace Config.Server.Application.Contracts.Operations.Config;

public static class SetConfigsBatch
{
    public sealed record Request(
        JsonElement Configs,
        string Project,
        string Profile,
        string Environment,
        string CreatedBy);
}