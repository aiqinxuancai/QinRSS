using Newtonsoft.Json;
using QinRSS.Service;
using QinRSS.Service.Model;
using System;


namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        class TESTD
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {

#if DEBUG

            List<TESTD> t = new List<TESTD>();
            t.Add(new TESTD() { Id = 1, Name = "1" });
            t.Add(new TESTD() { Id = 2, Name = "2" });

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new SubscriptionJsonContractResolver { includeClearTask = true };

            var content = JsonConvert.SerializeObject(t, settings);

            Console.WriteLine(content);

#endif


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