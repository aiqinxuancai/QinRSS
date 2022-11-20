using QinRSS.Service;
using System;


namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            //基础检查
            if (!File.Exists(AppConfig.ConfigPath))
            {
                Console.WriteLine($"{AppConfig.ConfigPath} 不存在，请创建配置文件");
                Console.ReadLine();
                return;
            }

            //启动ws服务
            await WebSocketManager.Instance.StartServiceT();

            //启动订阅推送服务
            SubscriptionManager.Instance.Restart();
            Console.ReadLine();

        }



    }
}