

using System;
using System.Diagnostics;
using System.Text.Json;

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
    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        public static AppConfigData Data { set; get; } = new AppConfigData();


        public static string ConfigPath => _configPath;

        private static string _configPath = Path.Combine(AppContext.BaseDirectory, "Config.json");

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

                Data = JsonSerializer.Deserialize<AppConfigData>(File.ReadAllText(_configPath));
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
                    File.WriteAllText(_configPath, JsonSerializer.Serialize(Data));
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
