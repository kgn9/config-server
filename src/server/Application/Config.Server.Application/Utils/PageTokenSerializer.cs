namespace Config.Server.Application.Utils;

public static class PageTokenSerializer
{
    private const int GuidSize = 16;

    public static string Serialize(DateTime lastDate, Guid lastId)
    {
        List<byte> tokenBytes = lastId.ToByteArray().ToList();
        tokenBytes.AddRange(BitConverter.GetBytes(lastDate.Ticks));

        return Convert.ToBase64String(tokenBytes.ToArray());
    }

    public static (DateTime LastDate, Guid LastId) Deserialize(string token)
    {
        byte[] data = Convert.FromBase64String(token);
        long ticks = BitConverter.ToInt64(data, GuidSize);
        byte[] guidBytes = new byte[GuidSize];
        Array.Copy(data, 0, guidBytes, 0, GuidSize);

        DateTime lastDate = new(ticks);
        Guid lastId = new(guidBytes);

        return (lastDate, lastId);
    }
}