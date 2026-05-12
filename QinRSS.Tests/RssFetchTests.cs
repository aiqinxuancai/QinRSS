using System.ServiceModel.Syndication;
using System.Xml;
using HtmlAgilityPack;
using Xunit;

namespace QinRSS.Tests;

/// <summary>
/// RSS 抓取与解析功能测试。
/// 无网络依赖的单元测试使用内嵌 XML；
/// 集成测试读取 TestConfig.yml 中的 rssHubUrl 作为目标地址。
/// </summary>
public class RssFetchTests
{
    // 内嵌示例 RSS，用于离线单元测试
    private const string SampleRssXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Feed</title>
            <link>https://example.com</link>
            <description>Test RSS Feed</description>
            <item>
              <title>Test Article 1</title>
              <link>https://example.com/article1</link>
              <description>&lt;p&gt;Hello &lt;b&gt;World&lt;/b&gt;&lt;/p&gt;&lt;img src="https://example.com/img1.jpg"/&gt;</description>
              <pubDate>Mon, 12 May 2025 10:00:00 +0000</pubDate>
            </item>
            <item>
              <title>Test Article 2</title>
              <link>https://example.com/article2</link>
              <description>&lt;p&gt;Another article&lt;/p&gt;</description>
              <pubDate>Mon, 12 May 2025 11:00:00 +0000</pubDate>
            </item>
          </channel>
        </rss>
        """;

    [Fact]
    public void ParseRssFeed_ValidXml_ReturnsFeedWithItems()
    {
        using var reader = XmlReader.Create(new StringReader(SampleRssXml));
        var feed = SyndicationFeed.Load(reader);

        Assert.NotNull(feed);
        Assert.Equal("Test Feed", feed.Title.Text);
        Assert.Equal(2, feed.Items.Count());
    }

    [Fact]
    public void ParseRssItem_ValidXml_ExtractsCorrectFields()
    {
        using var reader = XmlReader.Create(new StringReader(SampleRssXml));
        var feed = SyndicationFeed.Load(reader);
        var item = feed.Items.First();

        Assert.Equal("Test Article 1", item.Title.Text);
        Assert.Equal("https://example.com/article1", item.Links.First().Uri.ToString());
        Assert.NotNull(item.Summary);
    }

    [Fact]
    public void ParseRssItem_HtmlSummary_ExtractsPlainText()
    {
        using var reader = XmlReader.Create(new StringReader(SampleRssXml));
        var feed = SyndicationFeed.Load(reader);
        var item = feed.Items.First();

        var doc = new HtmlDocument();
        doc.LoadHtml(item.Summary.Text);
        var text = doc.DocumentNode.InnerText;

        Assert.Contains("Hello", text);
        Assert.Contains("World", text);
    }

    [Fact]
    public void ParseHtmlContent_ExtractsPlainText()
    {
        const string html = "<p>Hello <b>World</b></p>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var text = doc.DocumentNode.InnerText;

        Assert.Contains("Hello", text);
        Assert.Contains("World", text);
    }

    [Fact]
    public void ParseHtmlContent_ExtractsImageUrls()
    {
        const string html = "<div><p>Content</p><img src='https://example.com/img1.jpg'/><img src='https://example.com/img2.jpg'/></div>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var images = doc.DocumentNode.SelectNodes("//img");

        Assert.NotNull(images);
        Assert.Equal(2, images.Count);
        Assert.Equal("https://example.com/img1.jpg", images[0].Attributes["src"]?.Value);
        Assert.Equal("https://example.com/img2.jpg", images[1].Attributes["src"]?.Value);
    }

    [Fact]
    public void ParseHtmlContent_VideoWithPoster_ExtractsPosterUrl()
    {
        const string html = "<video poster='https://example.com/poster.jpg' src='video.mp4'></video>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var videos = doc.DocumentNode.SelectNodes("//video");

        Assert.NotNull(videos);
        Assert.Single(videos);
        Assert.Equal("https://example.com/poster.jpg", videos[0].Attributes["poster"]?.Value);
    }

    /// <summary>
    /// 集成测试：从真实 RSS 地址抓取订阅。
    /// 目标地址读取自 TestConfig.yml 的 rssHubUrl 字段。
    /// </summary>
    [SkippableFact]
    public void FetchRssFeed_RealUrl_ReturnsItems()
    {
        var url = TestSettings.Config.RssHubUrl;
        Skip.If(string.IsNullOrWhiteSpace(url), "TestConfig.yml 中 rssHubUrl 未填写，跳过 RSS 集成测试。");
        Skip.IfNot(Uri.IsWellFormedUriString(url, UriKind.Absolute), $"rssHubUrl 不是合法 URL：{url}");

        XmlReader? reader = null;
        try
        {
            reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);

            Assert.NotNull(feed);
            Assert.NotEmpty(feed.Items);
        }
        finally
        {
            reader?.Close();
        }
    }

    /// <summary>
    /// 集成测试：验证订阅标题可正常获取。
    /// </summary>
    [SkippableFact]
    public void FetchRssFeed_RealUrl_HasTitle()
    {
        var url = TestSettings.Config.RssHubUrl;
        Skip.If(string.IsNullOrWhiteSpace(url), "TestConfig.yml 中 rssHubUrl 未填写，跳过 RSS 集成测试。");
        Skip.IfNot(Uri.IsWellFormedUriString(url, UriKind.Absolute), $"rssHubUrl 不是合法 URL：{url}");

        XmlReader? reader = null;
        try
        {
            reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);

            Assert.NotNull(feed.Title?.Text);
            Assert.NotEmpty(feed.Title!.Text);
        }
        finally
        {
            reader?.Close();
        }
    }
}
