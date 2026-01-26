using Config.Server.Application.Models.Entities;
using Config.Server.Application.Models.Enums;

namespace Config.Server.Tests;

public class TestHelper
{
    public static ConfigItem CreateConfigItem(string key, string value)
    {
        return new ConfigItem(
            Id: 0,
            key,
            value,
            "TestProject1",
            "test1",
            [ConfigEnvironment.Global],
            DateTime.Today,
            DateTime.Today,
            "testCerator1");
    }
}