using Config.Server.Application.Models.Enums;
using System.Text.Json;

namespace Config.Server.Application.Contracts.Operations;

public static class SetConfigsBatch
{
    public sealed record Request(
        JsonElement Configs,
        string Project,
        string Profile,
        ConfigEnvironment Environment,
        string CreatedBy);
}