using AngelaAI.QQChannel.Service;
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

            //启动ws服务
            await WebSocketManager.Instance.StartServiceT();

            //启动订阅推送服务
            SubscriptionManager.Instance.Restart();
            Console.ReadLine();

        }



    }
}