namespace Config.Server.Configuration.Options;

public class ProviderOptions
{
    public string Project { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public int PageSize { get; set; }

    public int Cursor { get; set; }

    public TimeSpan UpdateInterval { get; set; }

    public string Url { get; set; } = string.Empty;
}