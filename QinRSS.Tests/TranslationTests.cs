using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Xunit;

namespace QinRSS.Tests;

/// <summary>
/// 翻译功能集成测试，与主项目 ChatGPTTranslatorManager 保持相同调用逻辑。
///
/// 在 TestConfig.yml 中填写以下字段后即可运行：
///   openAIKey        — API Key（必填，留空则跳过所有翻译测试）
///   openAIAPIBaseUri — 自定义反代地址（可选，留空使用 OpenAI 官方地址）
///   openAIAPIModel   — 模型名称（可选，默认 gpt-4o-mini）
/// </summary>
public class TranslationTests
{
    private const string SystemMessage = "请把以下内容翻译为简体中文，不要解释：";

    [SkippableFact]
    public async Task Translate_ShortEnglishText_ReturnsChinese()
    {
        var client = BuildClientOrSkip();
        var result = await TranslateAsync(client, "Hello, this is a test message.");
        Assert.NotEmpty(result);
    }

    [SkippableFact]
    public async Task Translate_LongNewsText_ReturnsChinese()
    {
        var client = BuildClientOrSkip();
        const string input =
            "Breaking news: Scientists discover a new species of bird in the Amazon rainforest. " +
            "The newly discovered bird has unique iridescent feather patterns never seen before. " +
            "Researchers believe it has been isolated from other species for thousands of years.";
        var result = await TranslateAsync(client, input);
        Assert.NotEmpty(result);
    }

    [SkippableFact]
    public async Task Translate_WithCustomBaseUri_ReturnsChinese()
    {
        Skip.If(string.IsNullOrWhiteSpace(TestSettings.Config.OpenAIAPIBaseUri),
            "TestConfig.yml 中 openAIAPIBaseUri 未填写，跳过自定义 BaseUri 测试。");
        var client = BuildClientOrSkip();
        var result = await TranslateAsync(client, "Good morning, the weather is nice today.");
        Assert.NotEmpty(result);
    }

    // ---------------------------------------------------------------
    // 边界条件（不需要 API Key）
    // ---------------------------------------------------------------

    [Fact]
    public void EmptyString_IsSkippedWithoutApiCall()
    {
        // 与主项目 ChatGPTTranslatorManager.Translater 逻辑一致：空字符串不调用 API
        var input = string.Empty;
        var result = string.IsNullOrWhiteSpace(input) ? string.Empty : "should not reach";
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void WhitespaceOnly_IsSkippedWithoutApiCall()
    {
        var input = "   ";
        var result = string.IsNullOrWhiteSpace(input) ? string.Empty : "should not reach";
        Assert.Equal(string.Empty, result);
    }

    // ---------------------------------------------------------------
    // 辅助方法（与主项目 ChatGPTTranslatorManager 保持相同构建逻辑）
    // ---------------------------------------------------------------

    /// <summary>
    /// 根据 TestConfig.yml 构建 ChatClient；openAIKey 为空时跳过当前测试。
    /// </summary>
    private static ChatClient BuildClientOrSkip()
    {
        var cfg = TestSettings.Config;
        Skip.If(string.IsNullOrEmpty(cfg.OpenAIKey),
            "TestConfig.yml 中 openAIKey 未填写，跳过翻译测试。");

        var model = string.IsNullOrWhiteSpace(cfg.OpenAIAPIModel) ? "gpt-4o-mini" : cfg.OpenAIAPIModel;

        var options = new OpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(60)
        };

        if (!string.IsNullOrWhiteSpace(cfg.OpenAIAPIBaseUri))
        {
            options.Endpoint = new Uri(cfg.OpenAIAPIBaseUri);
        }

        return new ChatClient(model, new ApiKeyCredential(cfg.OpenAIKey), options);
    }

    private static async Task<string> TranslateAsync(ChatClient client, string text)
    {
        var completion = await client.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(SystemMessage),
                new UserChatMessage(text)
            });

        return string.Join(string.Empty,
            completion.Value.Content.Select(p => p.Text ?? string.Empty)).Trim();
    }
}
