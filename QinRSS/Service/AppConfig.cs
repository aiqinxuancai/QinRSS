using AngelaAI.QQChannel.Service;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace MiraiAngelaAI.Service
{
    public class AppConfigData
    {
        /// <summary>
        /// ws监听地址 "ws://127.0.0.1:1089"
        /// </summary>
        public string WebSocketLocation { get; set; }

        /// <summary>
        /// RSSHUB站点地址如 https://rsshub.uneasy.win
        /// </summary>
        public string RSSHubUrl { get; set; }


        

    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        public static AppConfigData Data { set; get; } = new AppConfigData();

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
                if (File.Exists(_configPath) == false)
                {
                    InitDefault();
                    Save();
                    return false;
                }
                Data = JsonConvert.DeserializeObject<AppConfigData>(System.IO.File.ReadAllText(_configPath));
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
                    System.IO.File.WriteAllText(_configPath, JsonConvert.SerializeObject(Data, Formatting.None));
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
