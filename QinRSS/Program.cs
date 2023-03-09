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


            var r = await ChatGPTTranslatorManager.Translater("[(1/5)【景趣「二十四節気　清明・雛芥子」登場】清明の季節の収穫物を集めて交換できる景趣「二十四節気　清明・雛芥子(にじゅうしせっき　せいめい・ひなげし)」が登場します。＜実施期間＞2023年3月7日(火)メンテナンス終了時～2023年6月6日(火)メンテナンス開始前#刀剣乱舞 #とうらぶ https://t.co/QjsEoFJ9NA]");
            Console.WriteLine(r);
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