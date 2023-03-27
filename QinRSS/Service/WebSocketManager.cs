using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QinRSS.Service.OneBotModel;
using QinRSS.Utils;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace QinRSS.Service
{
    public partial class WebSocketManager
    {
        private WebSocketServer _server;

        private Dictionary<string, IWebSocketConnection> _connections = new Dictionary<string, IWebSocketConnection>();

        //最大存储100条包含echo的消息，让本地来获取
        private Dictionary<string, string> _messageEcho = new Dictionary<string, string>();


        private static WebSocketManager instance = new WebSocketManager();

        public static WebSocketManager Instance
        {
            get
            {
                return instance;
            }
        }

        public async Task StartServiceT()
        {
            SimpleLogger.Instance.Error($"WebSocketServer启动{AppConfig.Data.WebSocketLocation}");
            _server = new WebSocketServer(AppConfig.Data.WebSocketLocation);
            var server = _server;

            server.Start(socket =>
            {
                socket.OnOpen = () => Console.WriteLine("Open!");
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => {
                    JsonObject node = (JsonObject)JsonNode.Parse(message);

                    var echo = node.FirstOrDefault(a => a.Key == "echo");

                    if (!string.IsNullOrEmpty(echo.Value?.ToString()))
                    {
                        //Console.WriteLine(message);
                        _messageEcho[echo.Value?.ToString()] = message;
                    }

                    var postType = node.FirstOrDefault(a => a.Key == "post_type");
                    switch (postType.Value?.ToString())
                    {
                        case "meta_event":
                            {
                                OnMetaEvent(message, socket);
                                break;
                            }
                        case "message":
                            {
                                OnMessageEvent(message, socket);
                                break;
                            }
                    }
                   
                };
            });
        }

        /// <summary>
        /// 收消息
        /// </summary>
        /// <param name="message"></param>
        private async void OnMessageEvent(string message, IWebSocketConnection webSocketConnection)
        {
            JsonObject node = (JsonObject)JsonNode.Parse(message);

            //var postType = node.FirstOrDefault(a => a.Key == "post_type");  //meta_event\message
            var messageType = node.FirstOrDefault(a => a.Key == "message_type");
            var subType = node.FirstOrDefault(a => a.Key == "sub_type");


            //Console.WriteLine(message);
            //Console.WriteLine(messageType.Value?.ToString());
            //Console.WriteLine(subType.Value?.ToString());

            var messageTypeStr = messageType.Value?.ToString();


            switch (messageTypeStr)
            {
                case "group":
                    {
                        switch (subType.Value?.ToString())
                        {
                            case "normal":
                                {
                                    //消息种类和方法太多了，所以不使用DeserializeObject反序列化了

                                    //var obj = JsonConvert.DeserializeObject<OneBotGroupNormalMessage>(message);

                                    JsonObject obj = JsonNode.Parse(message) as JsonObject;

                                    //TODO 根据self_id来处理
                                    await OnOneBotGroupNormalMessage(obj, webSocketConnection);
                                    break;
                                }
                        }

                        break;
                    }
                case "guild":
                    {
                        switch (subType.Value?.ToString())
                        {
                            case "channel":
                                {
                                    //var obj = JsonConvert.DeserializeObject<OneBotGroupChannelMessage>(message);
                                    //TODO 根据self_id来处理
                                    JsonObject obj = JsonNode.Parse(message) as JsonObject;
                                    //JObject obj = JObject.Parse(message);
                                    await OnOneBotGroupChannelMessage(obj, webSocketConnection);
                                    break;
                                }
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        private void OnMetaEvent(string message, IWebSocketConnection webSocketConnection)
        {
            JsonObject node = (JsonObject)JsonNode.Parse(message);

            var messageType = node.FirstOrDefault(a => a.Key == "meta_event_type");
            var subType = node.FirstOrDefault(a => a.Key == "sub_type");
            var metaEventType = node.FirstOrDefault(a => a.Key == "meta_event_type");
            

            if (metaEventType.Value?.ToString() == "lifecycle")
            {
                switch (subType.Value?.ToString())
                {
                    case "connect":
                        {
                            var selfId = ((ulong)node["self_id"]);
                            _connections[selfId.ToString()] = webSocketConnection;
                            Console.WriteLine($"{selfId}已连接");
                            break;
                        }
                    case "enable":
                        {
                            break;
                        }
                    case "disable":
                        {
                            break;
                        }
                }

            }


        }

        /// <summary>
        /// 普通群处理
        /// </summary>
        /// <param name="message"></param>
        /// <param name="webSocketConnection"></param>
        private async Task OnOneBotGroupNormalMessage(JsonObject message, IWebSocketConnection webSocketConnection)
        {
            var messageContent = message["raw_message"]?.ToString();

            var cmd = messageContent.Split(" ");

            var selfId = message["self_id"]?.ToString();
            var groupId = (long)message["group_id"];
            var userId = (long)message["user_id"];


            string[] actions = { "#add", "#remove", "#clear", "#list", "#help", "#test" }; 

           
            if (!actions.Any(a => messageContent.StartsWith(a)))
            {
                return; //非命令不处理
            }

            var memberInfo = await GetGroupMemberInfo(webSocketConnection, groupId, userId);
            var isQinESSAdmin = AppConfig.Data.GroupAdmins.Any(a => a == userId);

            if (memberInfo.Data.Role != "owner" && !isQinESSAdmin)
            {
                return; //不是群主不处理
            }

            await OnAdminCommand(webSocketConnection, selfId, messageContent, "", $"{groupId}");
        }


        private async Task OnOneBotGroupChannelMessage(JsonObject message, IWebSocketConnection webSocketConnection)
        {

            JsonNode m = message["message"];
            JsonNode p = message["post_type"];
            var messageContent = "";


            if (m.GetType() == typeof(JsonArray))
            {
                var textObj = m.AsArray().FirstOrDefault(a => (string)a["type"] == "text");
                if (textObj != null)
                {
                    messageContent = (string)textObj["data"]["text"];
                }
            } 
            else if (m.GetValue<JsonElement>().ValueKind == JsonValueKind.String)
            {
                messageContent = (string)m;
            }
           


            var cmd = messageContent.Split(" ");

            var selfId = message["self_id"]?.ToString();
            var guildId = (string)message["guild_id"];
            var userId = message["sender"]["user_id"].ToString();
            var channelId = (string)message["channel_id"];


            string[] actions = { "#add", "#remove", "#clear", "#list", "#help", "#test" };


            if (!actions.Any(a => messageContent.StartsWith(a)))
            {
                return; //非命令不处理
            }

            var memberInfo = await GetGuildMetaByGuest(webSocketConnection, guildId);
            var isQinESSAdmin = AppConfig.Data.GuildAdmins.Any(a => a == userId);
            if (memberInfo.Data.OwnerId != userId && !isQinESSAdmin)
            {
                return; //不是群主不处理
            }


            await OnAdminCommand(webSocketConnection, selfId, messageContent, guildId, channelId);
        }

        private async Task OnAdminCommand(IWebSocketConnection webSocketConnection, string selfId, string message, string guildId, string groupOrchannelId)
        {
            var args = message.Split(" ");
            var commandName = args.FirstOrDefault();
            var returnString = "";

            

            switch (commandName)
            {
                case "#add":
                    {
                        if (args.Length >= 3)
                        {
                            try
                            {
                                //翻译内容
                                bool translate = args.Any(a => a == "--translate");
                                bool translateOnly = args.Any(a => a == "--translateOnly");

                                if (SubscriptionManager.Instance.Add(selfId, guildId, groupOrchannelId, args[1], args[2], translate, translateOnly))
                                {
                                    returnString = $"已添加订阅{args[1]}";
                                }
                            }
                            catch (Exception ex)
                            {
                                returnString = ex.Message;
                            }
                        }
                        break;
                    }
                case "#remove":
                    {
                        if (args.Length >= 2)
                        {
                            SubscriptionManager.Instance.Remove(selfId, guildId, groupOrchannelId, args[1]);
                            returnString = $"已移除订阅{args[1]}";
                        }
                        break;
                    }
                case "#clear":
                    {
                        if (args.Length >= 1)
                        {
                            SubscriptionManager.Instance.Clear(selfId, guildId, groupOrchannelId);
                            returnString = $"已清空订阅{args[1]}";
                        }
                        break;
                    }
                case "#list":
                    {
                        if (args.Length >= 1)
                        {
                            var list = SubscriptionManager.Instance.List(selfId, guildId, groupOrchannelId);
                            if (!string.IsNullOrEmpty(list))
                            {
                                returnString = list;
                            }
                            else
                            {
                                returnString = "列表为空";
                            }

                        }
                        break;
                    }
                case "#test":
                    {
                        if (args.Length >= 1)
                        {
                            //var base64 = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\aiqin\Pictures\Elden_Ring_cover.png"));

         
                            returnString = $"""
                                Twitter @刀剣乱舞-本丸通信-【公式】
                                RT 刀剣乱舞STORE【残り4日】ご好評頂いております「刀剣乱舞STORE KYOTO」は11月27日(日)まで開催！新商品を多数取り揃えているほか、購入特典もございますのでどうぞお見逃しなく…！皆様のご来店をお待ちしております。STORE▶#刀剣乱舞 #とうらぶ https://www.toulove-store.com/index_kyoto.html

                                [CQ:image,file=https://pbs.twimg.com/media/FiTTwKJUYAEDfnB?format=jpg&name=orig]
                                [CQ:image,file=https://pbs.twimg.com/media/FiTTxKjVUAEYQSr?format=jpg&name=orig]
                                [CQ:image,file=https://pbs.twimg.com/media/FiTTyCjUoAEZp_G?format=jpg&amp;name=orig]
                                [CQ:image,file=https://pbs.twimg.com/media/FiTTzAGUAAEADji?format=jpg&amp;name=orig]
                                更新时间：2022-11-24 13:00:01
                                链接：https://twitter.com/toulove_store/status/1595643376472182784
                                
                                """;
                            returnString = HttpUtility.HtmlDecode(returnString);
                            returnString = await DownloadHelper.MessageImageUrlToBase64(returnString);

                        }
                        break;
                    }
                case "#help":
                    {
                        returnString = """
                            增加订阅
                            #add [自定义名称] [订阅地址如twitter/user/TOUKEN_STAFF]
                            可在尾部附加参数为 --translate 发送翻译到中文后的内容

                            删除订阅
                            #remove [自定义名称]  

                            清除本频道、群中所有订阅
                            #clear 

                            查看订阅
                            #list

                            更多内容请前往 https://github.com/aiqinxuancai/QinRSS
                            """;
                        break;
                    }

            }

            if (!string.IsNullOrEmpty(returnString))
            {
                if (string.IsNullOrEmpty(guildId))
                {
                    //群消息
                    await SendGroupMessage(webSocketConnection, groupOrchannelId, returnString);
                }
                else
                {
                    //频道消息
                    await SendChannelMessage(webSocketConnection, guildId, groupOrchannelId, returnString);
                }
            }



        }
    }
}
