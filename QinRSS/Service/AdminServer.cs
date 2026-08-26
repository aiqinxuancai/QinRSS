using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace QinRSS.Service;

/// <summary>QinRSS 内置 Kestrel 管理服务。</summary>
public sealed class AdminServer
{
    private readonly string _webRoot = Path.Combine(AppContext.BaseDirectory, "AdminWeb");
    private readonly object _serverLock = new();
    private WebApplication? _app;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public void Start()
    {
        lock (_serverLock)
        {
            if (_app != null) return;
            var host = string.IsNullOrWhiteSpace(AppConfig.Data.AdminHost) ? "0.0.0.0" : AppConfig.Data.AdminHost;
            var port = Math.Clamp(AppConfig.Data.AdminPort, 1, 65535);
            // Standard builder keeps reflection-based serializers used by the existing
            // cache and subscription models compatible with the embedded host.
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://{host}:{port}");
            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.AccessControlAllowOrigin = "*";
                try { await next(); }
                catch (Exception ex)
                {
                    SimpleLogger.Instance.Error($"管理请求失败：{ex}");
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new { error = ex.Message }, JsonOptions);
                    }
                }
            });

            var files = new PhysicalFileProvider(_webRoot);
            app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
            app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
            MapApi(app);

            app.StartAsync().GetAwaiter().GetResult();
            _app = app;
            SimpleLogger.Instance.Info($"管理界面已启动：http://{host}:{port}/");
        }
    }

    public async Task Restart()
    {
        WebApplication? old;
        lock (_serverLock) { old = _app; _app = null; }
        if (old != null)
        {
            try { await old.StopAsync(); } catch { }
            await old.DisposeAsync();
        }
        Start();
    }

    private void MapApi(WebApplication app)
    {
        app.MapGet("/api/status", () =>
        {
            var subscriptions = SubscriptionManager.Instance.SubscriptionModel
                .SelectMany(bot => bot.AllSubscription.Select(item => new
                {
                    selfId = bot.SelfId, item.Url, item.Name, item.CustomName, item.GroupOrChannelId,
                    item.GuildId, item.Translate, item.TranslateOnly,
                    subscriptionType = item.SubscriptionType.ToString(), item.TaskFullCount
                })).ToArray();
            return Results.Json(new
            {
                connected = WebSocketManager.Instance.ConnectedSelfIds,
                subscriptions,
                subscriptionCount = subscriptions.Length,
                lastSentTime = AppCache.Data.LastSentTime,
                dataDir = AppConfig.DataDir,
                configPath = AppConfig.ConfigPath,
                adminPort = AppConfig.Data.AdminPort,
                config = new { AppConfig.Data.RunInterval },
                logFile = SimpleLogger.Instance.LogFilename
            }, JsonOptions);
        });

        app.MapGet("/api/config", () => Results.Json(AppConfig.Data, JsonOptions));
        app.MapGet("/api/logs", (HttpContext context) => Results.Json(new
        {
            content = SimpleLogger.Instance.ReadRecent(ParseInt(context.Request.Query["lines"], 300)),
            file = SimpleLogger.Instance.LogFilename
        }, JsonOptions));

        app.MapPost("/api/config", async (HttpContext context) =>
        {
            var data = await context.Request.ReadFromJsonAsync<AppConfigData>(JsonOptions)
                ?? throw new InvalidOperationException("配置内容不能为空");
            var oldWebSocket = AppConfig.Data.WebSocketLocation;
            var oldAdminHost = AppConfig.Data.AdminHost;
            var oldAdminPort = AppConfig.Data.AdminPort;
            AppConfig.Apply(data);
            ChatGPTTranslatorManager.Reload();
            SubscriptionManager.Instance.Restart();
            if (!string.Equals(oldWebSocket, data.WebSocketLocation, StringComparison.OrdinalIgnoreCase))
                await WebSocketManager.Instance.Restart();

            var adminChanged = oldAdminPort != data.AdminPort || !string.Equals(oldAdminHost, data.AdminHost, StringComparison.OrdinalIgnoreCase);
            if (adminChanged)
                _ = Task.Run(async () => { await Task.Delay(600); await Restart(); });
            return Results.Json(new
            {
                ok = true,
                message = "配置已保存并实时生效",
                restartUrl = adminChanged ? $"http://{PublicHost(data.AdminHost)}:{data.AdminPort}/" : null
            }, JsonOptions);
        });

        app.MapPost("/api/subscriptions", async (HttpContext context) =>
        {
            var body = await context.Request.ReadFromJsonAsync<SubscriptionRequest>(JsonOptions)
                ?? throw new InvalidOperationException("订阅内容不能为空");
            SubscriptionManager.Instance.Add(body.SelfId ?? string.Empty, body.GuildId ?? string.Empty,
                body.TargetId ?? string.Empty, body.Name ?? string.Empty, body.Url ?? string.Empty,
                body.Translate, body.TranslateOnly);
            return Results.Json(new { ok = true, message = "订阅已添加" }, JsonOptions);
        });

        app.MapDelete("/api/subscriptions", (HttpContext context) =>
        {
            var query = context.Request.Query;
            SubscriptionManager.Instance.Remove(query["selfId"].ToString(), query["guildId"].ToString(),
                query["targetId"].ToString(), query["name"].ToString());
            return Results.Json(new { ok = true, message = "订阅已删除" }, JsonOptions);
        });

        app.MapPost("/api/subscriptions/refresh", () =>
        {
            SubscriptionManager.Instance.Restart();
            return Results.Json(new { ok = true, message = "订阅任务已刷新" }, JsonOptions);
        });

        app.MapPost("/api/send", async (HttpContext context) =>
        {
            var body = await context.Request.ReadFromJsonAsync<SendRequest>(JsonOptions)
                ?? throw new InvalidOperationException("消息内容不能为空");
            if (string.IsNullOrWhiteSpace(body.SelfId) || string.IsNullOrWhiteSpace(body.TargetId) || string.IsNullOrWhiteSpace(body.Message))
                throw new InvalidOperationException("Bot、目标和消息均不能为空");
            if (!WebSocketManager.Instance.ConnectedSelfIds.Contains(body.SelfId))
                throw new InvalidOperationException("指定 Bot 当前未连接");
            if (string.Equals(body.TargetType, "channel", StringComparison.OrdinalIgnoreCase))
                await WebSocketManager.Instance.SendChannelMessage(body.SelfId, body.GuildId ?? string.Empty, body.TargetId, body.Message);
            else
                await WebSocketManager.Instance.SendGroupMessage(body.SelfId, body.TargetId, body.Message);
            SimpleLogger.Instance.Info($"管理界面发送消息：{body.SelfId} -> {body.TargetId}");
            return Results.Json(new { ok = true, message = "消息已发送" }, JsonOptions);
        });
    }

    private static int ParseInt(string? value, int fallback) => int.TryParse(value, out var number) ? Math.Clamp(number, 1, 2000) : fallback;
    private static string PublicHost(string? host) => host is null or "" or "0.0.0.0" or "*" or "+" ? "127.0.0.1" : host;

    private sealed class SubscriptionRequest
    {
        public string? SelfId { get; set; }
        public string? GuildId { get; set; }
        public string? TargetId { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public bool Translate { get; set; }
        public bool TranslateOnly { get; set; }
    }

    private sealed class SendRequest
    {
        public string? SelfId { get; set; }
        public string? TargetType { get; set; }
        public string? GuildId { get; set; }
        public string? TargetId { get; set; }
        public string? Message { get; set; }
    }
}
