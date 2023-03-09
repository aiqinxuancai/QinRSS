

using System;
using System.Diagnostics;
using System.Text.Json;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace QinRSS.Service
{
    public class AppConfigData
    {
        /// <summary>
        /// ws监听地址 如"ws://127.0.0.1:1089"
        /// </summary>
        public string WebSocketLocation { get; set; } = string.Empty;

        /// <summary>
        /// RSSHUB站点地址 如"https://rsshub.app"
        /// </summary>
        [YamlMember(Alias = "rssHubUrl", ApplyNamingConventions = false)]
        public string RSSHubUrl { get; set; } = string.Empty;

        /// <summary>
        /// QQ群管理ID
        /// </summary>
        public long[] GroupAdmins { get; set; } = new long[0];

        /// <summary>
        /// QQ频道管理员ID
        /// </summary>
        public string[] GuildAdmins { get; set; } = new string[0];

        /// <summary>
        /// 离线大于1天后再上线，首次不发送订阅，避免出现消息轰炸
        /// </summary>
        public bool NotSentAfterLongOffline { get; set; }

        /// <summary>
        /// 检查订阅的时间间隔（秒），建议大于60秒，具体更新速度可能取决于RSSHub站点的设置
        /// </summary>
        public int RunInterval { get; set; } = 60;


        /// <summary>
        /// 在插件中将图片下载后再进行发送，而非直接传递URL，避免部分情况go-cqhttp自身问题导致的图片无法正常发送
        /// </summary>
        public bool SelfDownloadImage { get; set; }

        /// <summary>
        /// 图片代理，设置后使用代理下载图片发送，如 http://127.0.0.1:1080，仅在SelfDownloadImage设置为true时可用
        /// </summary>
        public string ImageProxy { get; set; } = string.Empty;

        /// <summary>
        /// OpenAI-Key，用于翻译内容时调用OpenAI
        /// </summary>
        [YamlMember(Alias = "openAIKey", ApplyNamingConventions = false)]
        public string OpenAIKey { get; set; } = string.Empty;

        /// <summary>
        /// 用于无法连接OpenAI的情况
        /// </summary>
        [YamlMember(Alias = "openAIProxy", ApplyNamingConventions = false)]
        public string OpenAIProxy { get; set; } = string.Empty;

        
    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        
        public static AppConfigData Data { set; get; } = new AppConfigData();


        public static string ConfigPath => _configPath;

        private static string _configPath = Path.Combine(AppContext.BaseDirectory, "Config.yml");

        private static object _lock = new object();

        static AppConfig()
        {
            Init();
        }

        public static void InitDefault() //载入默认配置
        {
        }


        public static bool Init()
        {
            try
            {
                SimpleLogger.Instance.Info($"初始化配置{_configPath}");
                if (File.Exists(_configPath) == false)
                {
                    InitDefault();
                    Save();
                    return false;
                }
                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .Build();

                var p = deserializer.Deserialize<AppConfigData>(File.ReadAllText(_configPath));

                Data = p;
                //Data = JsonSerializer.Deserialize<AppConfigData>(File.ReadAllText(_configPath));
                SimpleLogger.Instance.Info($"配置初始化完成");
                return true;
            }
            catch (System.Exception ex)
            {
                SimpleLogger.Instance.Info($"配置初始化失败：{ex}");
                return false;
            }
        }


        public static void Save()
        {
            try
            {
                lock(_lock)
                {

                    var serializer = new SerializerBuilder().Build();
                    var yaml = serializer.Serialize(Data);
                    //System.Console.WriteLine(yaml);
                    File.WriteAllText(_configPath, yaml);
                    //File.WriteAllText(_configPath, JsonSerializer.Serialize(Data));
                    Console.WriteLine($"配置已经存储");
                }
                
            }
            catch (System.Exception ex)
            {
                SimpleLogger.Instance.Info($"配置存储失败：{ex}");
            }
        }


    }
}
