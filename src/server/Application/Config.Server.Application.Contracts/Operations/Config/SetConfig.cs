namespace Config.Server.Application.Contracts.Operations.Config;

public class SetConfig
{
    public sealed record Request(
        string Project,
        string Profile,
        string Environment,
        string Value,
        string Key,
        string CreatedBy);
}