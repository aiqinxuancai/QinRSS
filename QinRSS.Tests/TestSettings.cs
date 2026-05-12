using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QinRSS.Tests;

/// <summary>
/// 测试配置数据，字段与主项目 Config.yml 保持一致。
/// </summary>
public class TestConfigData
{
    /// <summary>RSS 抓取集成测试地址</summary>
    [YamlMember(Alias = "rssHubUrl", ApplyNamingConventions = false)]
    public string RssHubUrl { get; set; } = "https://news.ycombinator.com/rss";

    /// <summary>OpenAI-Key，留空则跳过翻译相关测试</summary>
    [YamlMember(Alias = "openAIKey", ApplyNamingConventions = false)]
    public string OpenAIKey { get; set; } = string.Empty;

    /// <summary>翻译接口反代地址，留空使用 OpenAI 官方地址</summary>
    [YamlMember(Alias = "openAIAPIBaseUri", ApplyNamingConventions = false)]
    public string OpenAIAPIBaseUri { get; set; } = string.Empty;

    /// <summary>翻译使用的模型名称</summary>
    [YamlMember(Alias = "openAIAPIModel", ApplyNamingConventions = false)]
    public string OpenAIAPIModel { get; set; } = "gpt-4o-mini";
}

/// <summary>
/// 从 TestConfig.yml 读取测试配置的静态入口。
/// </summary>
public static class TestSettings
{
    public static readonly TestConfigData Config;

    static TestSettings()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "TestConfig.yml");
        if (File.Exists(configPath))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            Config = deserializer.Deserialize<TestConfigData>(File.ReadAllText(configPath))
                     ?? new TestConfigData();
        }
        else
        {
            Config = new TestConfigData();
        }
    }
}
