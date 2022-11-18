using AngelaAI.QQChannel.Service;
using Fleck;
using MiraiAngelaAI.Service;
using Newtonsoft.Json;
using QinRSS.Service.OneBotModel;
using System.Buffers.Text;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using WatsonWebsocket;

namespace QinRSS.Service
{
    public partial class WebSocketManager
    {
        /// <summary>
        /// WS客户端
        /// </summary>
        private WatsonWsServer Server { get; set; }

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
            var server = new WebSocketServer(AppConfig.Data.WebSocketLocation);
            server.Start(socket =>
            {
                socket.OnOpen = () => Console.WriteLine("Open!");
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => {
                    JsonObject node = (JsonObject)JsonNode.Parse(message);

                    var echo = node.FirstOrDefault(a => a.Key == "echo");

                    if (!string.IsNullOrEmpty(echo.Value?.ToString()))
                    {
                        Console.WriteLine(message);
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


            Console.WriteLine(message);
            Console.WriteLine(messageType.Value?.ToString());
            Console.WriteLine(subType.Value?.ToString());

            var messageTypeStr = messageType.Value?.ToString();


            switch (messageTypeStr)
            {
                case "group":
                    {
                        switch (subType.Value?.ToString())
                        {
                            case "normal":
                                {
                                    var obj = JsonConvert.DeserializeObject<OneBotGroupNormalResponse>(message);
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
                                    var obj = JsonConvert.DeserializeObject<OneBotGroupChannelResponse>(message);
                                    //TODO 根据self_id来处理
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
        private async Task OnOneBotGroupNormalMessage(OneBotGroupNormalResponse message, IWebSocketConnection webSocketConnection)
        {
            var cmd = message.Message.Split(" ");

            var selfId = $"{message.SelfId}";
            var groupId = $"{message.GroupId}";

            string[] actions = { "#add", "#remove", "#clear", "#list" }; 

           
            if (!actions.Any(a => message.Message.StartsWith(message.Message)))
            {
                return; //非命令不处理
            }

            var memberInfo = await GetGroupMemberInfo(webSocketConnection, message.GroupId, message.UserId);
            if (memberInfo.Data.Role != "owner")
            {
                return; //不是群主不处理
            }

            switch (cmd.FirstOrDefault())
            {
                case "#add":
                    {
                        if (cmd.Length == 3)
                        {
                            try {
                                if (SubscriptionManager.Instance.Add(selfId, "", groupId, cmd[1], cmd[2]))
                                {
                                    await SendGroupMessage(webSocketConnection, message.GroupId, $"已添加订阅{cmd[1]}");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SendGroupMessage(webSocketConnection, message.GroupId, ex.Message);
                            }
                        }
                        break;
                    }
                case "#remove":
                    {
                        if (cmd.Length == 2)
                        {
                            SubscriptionManager.Instance.Remove(selfId, "", groupId, cmd[1]);
                            await SendGroupMessage(webSocketConnection, message.GroupId, $"已移除订阅{cmd[1]}");
                        }
                        break;
                    }
                case "#clear":
                    {
                        if (cmd.Length == 1)
                        {
                            SubscriptionManager.Instance.Clear(selfId, "", groupId);
                            await SendGroupMessage(webSocketConnection, message.GroupId, $"已清空订阅{cmd[1]}");
                        }
                        break;
                    }
                case "#list":
                    {
                        if (cmd.Length >= 1)
                        {
                            var list = SubscriptionManager.Instance.List(selfId, "", groupId);
                            if (!string.IsNullOrEmpty(list))
                            {
                                await SendChannelMessage(webSocketConnection, "", groupId, list);
                            }
                            else
                            {
                                await SendChannelMessage(webSocketConnection, "", groupId, "列表为空");
                            }

                        }
                        break;
                    }
                case "#test":
                    {
                        if (cmd.Length == 1)
                        {
                            //var base64 = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\aiqin\Pictures\Elden_Ring_cover.png"));
                            //var memberInfo = await GetGroupMemberInfo(webSocketConnection, message.GroupId, message.UserId);
                            //if (memberInfo.Data.Role == "owner")
                            //{
                            //    await SendGroupMessage(webSocketConnection, message.GroupId, $"test[CQ:image,file=base64://{base64}]");
                            //}
                            
                        }
                        break;
                    }
            }
        }


        private async Task OnOneBotGroupChannelMessage(OneBotGroupChannelResponse message, IWebSocketConnection webSocketConnection)
        {
            var cmd = message.Message.Split(" ");

            var selfId = $"{message.SelfId}";
 
            string[] actions = { "#add", "#remove", "#clear", "#list", "#test"};


            if (!actions.Any(a => message.Message.StartsWith(message.Message)))
            {
                return; //非命令不处理
            }

            var memberInfo = await GetGuildMetaByGuest(webSocketConnection, message.GuildId);
            if (memberInfo.Data.OwnerId != message.Sender.UserId)
            {
                return; //不是群主不处理
            }

            switch (cmd.FirstOrDefault())
            {
                case "#add":
                    {
                        if (cmd.Length >= 3)
                        {
                            try
                            {
                                if (SubscriptionManager.Instance.Add(selfId, message.GuildId, message.ChannelId, cmd[1], cmd[2]))
                                {
                                    await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, $"已添加订阅{cmd[1]}");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, ex.Message);
                            }
                        }
                        break;
                    }
                case "#remove":
                    {
                        if (cmd.Length >= 2)
                        {
                            SubscriptionManager.Instance.Remove(selfId, message.GuildId, message.ChannelId, cmd[1]);
                            await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, $"已移除订阅{cmd[1]}");
                        }
                        break;
                    }
                case "#clear":
                    {
                        if (cmd.Length >= 1)
                        {
                            SubscriptionManager.Instance.Clear(selfId, message.GuildId, message.ChannelId);
                            await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, $"已清空订阅{cmd[1]}");
                        }
                        break;
                    }
                case "#list":
                    {
                        if (cmd.Length >= 1)
                        {
                            var list = SubscriptionManager.Instance.List(selfId, message.GuildId, message.ChannelId);
                            if (!string.IsNullOrEmpty(list))
                            {
                                await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, list);
                            } 
                            else
                            {
                                await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, "列表为空");
                            }
                            
                        }
                        break;
                    }
                case "#test":
                    {
                        if (cmd.Length >= 1)
                        {
                            var base64 = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\aiqin\Pictures\Elden_Ring_cover.png"));
                            await SendChannelMessage(webSocketConnection, message.GuildId, message.ChannelId, $"test[CQ:image,file=base64://{base64}]");

                        }
                        break;
                    }
            }
        }


    }
}
