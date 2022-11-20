
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace QinRSS.Service
{
    [AddINotifyPropertyChangedInterface]
    public class AppCacheData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(PropertyName);
            PropertyChanged(this, propertyChangedEventArgs);
            AppCache.Save();
        }

        /// <summary>
        /// 最后一次发送订阅的时间
        /// </summary>
        public DateTime? LastSentTime { get; set; }


    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppCache
    {
        public static AppCacheData Data { set; get; } = new AppCacheData();

        private static string _configPath = Path.Combine(AppContext.BaseDirectory, "Cache.json");

        private static object _lock = new object();

        static AppCache()
        {
            Init();
        }

        public static void InitDefault() //载入默认
        {
        }

        private static bool Init()
        {
            try
            {
                if (File.Exists(_configPath) == false)
                {
                    InitDefault();
                    Save();
                    return false;
                }
                Data = JsonSerializer.Deserialize<AppCacheData>(File.ReadAllText(_configPath));
                return true;
            }
            catch (System.Exception ex)
            {
                SimpleLogger.Instance.Info($"Cache初始化失败：{ex}");
                return false;
            }
        }


        internal static void Save()
        {
            try
            {
                lock(_lock)
                {
                    File.WriteAllText(_configPath, JsonSerializer.Serialize(Data));
                    Console.WriteLine($"Cache已经存储");
                }
                
            }
            catch (System.Exception ex)
            {
                SimpleLogger.Instance.Info($"Cache存储失败：{ex}");
            }
        }


    }
}
