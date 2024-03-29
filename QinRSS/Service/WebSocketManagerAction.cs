﻿using Fleck;
using Newtonsoft.Json;
using QinRSS.Service.OneBotModel;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace QinRSS.Service
{
    public partial class WebSocketManager
    {


        public async Task SendPrivateMessage(IWebSocketConnection webSocketConnection, long userId, string text)
        {
            JsonObject json = new JsonObject();
            json["action"] = "send_private_msg";//"action": "get_self_info", //send_group_msg
            json["params"] = new JsonObject();
            json["params"]["detail_type"] = "private";
            json["params"]["user_id"] = $"{userId}";
            json["params"]["message"] = text;
            //json["echo"] = Guid.NewGuid().ToString();
            var send = json.ToJsonString();
            Console.WriteLine(send);
            await webSocketConnection.Send(send);
        }

        public async Task SendGroupMessage(IWebSocketConnection webSocketConnection, string groupId, string text)
        {
            JsonObject json = new JsonObject();
            json["action"] = "send_group_msg";//"action": "get_self_info", //send_group_msg
            json["params"] = new JsonObject();
            json["params"]["group_id"] = $"{groupId}";
            //json["params"]["auto_escape"] = true;
            json["params"]["message"] = text;
            //json["echo"] = Guid.NewGuid().ToString();
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);
        }

        public async Task SendGroupMessage(string selfId, string groupId, string text)
        {
            if (_connections.ContainsKey(selfId))
            {
                IWebSocketConnection webSocketConnection = _connections[selfId];

                if (webSocketConnection.IsAvailable)
                {
                    SendGroupMessage(webSocketConnection, groupId, text);
                }
            }
        }

        public async Task SendChannelMessage(IWebSocketConnection webSocketConnection, string guildId, string channelId, string text)
        {
            JsonObject json = new JsonObject();
            json["action"] = "send_guild_channel_msg";
            json["params"] = new JsonObject();
            json["params"]["guild_id"] = $"{guildId}";
            json["params"]["channel_id"] = $"{channelId}";
            json["params"]["message"] = text;
            //json["echo"] = Guid.NewGuid().ToString();
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);

        }

        public async Task SendChannelMessage(string selfId, string guildId, string channelId, string text)
        {
            if (_connections.ContainsKey(selfId))
            {
                IWebSocketConnection webSocketConnection = _connections[selfId];

                if (webSocketConnection.IsAvailable)
                {
                    await SendChannelMessage(webSocketConnection, guildId, channelId, text);
                }
            }
        }

        public async Task<OneBotGroupMemberInfoResponse> GetGroupMemberInfo(IWebSocketConnection webSocketConnection, long groupId, long userId)
        {
            JsonObject json = new JsonObject();
            json["action"] = "get_group_member_info";
            json["params"] = new JsonObject();
            json["params"]["group_id"] = $"{groupId}";
            json["params"]["user_id"] = $"{userId}";

            var echo = Guid.NewGuid().ToString();
            json["echo"] = echo;
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);
            var message = await WaitEcho(echo);

            if (message.Item1 == 0)
            {
                OneBotGroupMemberInfoResponse obj = JsonConvert.DeserializeObject<OneBotGroupMemberInfoResponse>(message.Item2);
                return obj;
            }
            else
            {
                //失败
            }

            return null;
        }

        /// <summary>
        /// 获取频道成员信息
        /// </summary>
        /// <param name="webSocketConnection"></param>
        /// <param name="guildId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<OneBotGroupMemberInfoResponse> GetGuildMemeberProfile(IWebSocketConnection webSocketConnection, string guildId, string userId)
        {
            //get_guild_member_profile
            JsonObject json = new JsonObject();
            json["action"] = "get_guild_member_profile";
            json["params"] = new JsonObject();
            json["params"]["guild_id"] = $"{guildId}";
            json["params"]["user_id"] = $"{userId}";

            var echo = Guid.NewGuid().ToString();
            json["echo"] = echo;
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);
            var message = await WaitEcho(echo);

            if (message.Item1 == 0)
            {
                OneBotGroupMemberInfoResponse obj = JsonConvert.DeserializeObject<OneBotGroupMemberInfoResponse>(message.Item2);
                return obj;
            }
            else
            {
                //失败
            }

            return null;
        }

        public async Task<OneBotGroupMemberInfoResponse> GetGuildList(IWebSocketConnection webSocketConnection)
        {
            //get_guild_list
            JsonObject json = new JsonObject();
            json["action"] = "get_guild_list";
            json["params"] = new JsonObject();
            var echo = Guid.NewGuid().ToString();
            json["echo"] = echo;
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);
            var message = await WaitEcho(echo);

            if (message.Item1 == 0)
            {
                OneBotGroupMemberInfoResponse obj = JsonConvert.DeserializeObject<OneBotGroupMemberInfoResponse>(message.Item2);
                return obj;
            }
            else
            {
                //失败
            }

            return null;
        }

        public async Task<OnBotGetGuildMetaByGuestResponse> GetGuildMetaByGuest(IWebSocketConnection webSocketConnection, string guildId)
        {
            //get_guild_list
            JsonObject json = new JsonObject();
            json["action"] = "get_guild_meta_by_guest";
            json["params"] = new JsonObject();
            json["params"]["guild_id"] = $"{guildId}";
            var echo = Guid.NewGuid().ToString();
            json["echo"] = echo;

           
            var send = json.ToJsonString();
            //Console.WriteLine(send);
            await webSocketConnection.Send(send);
            var message = await WaitEcho(echo);

            if (message.Item1 == 0)
            {
                OnBotGetGuildMetaByGuestResponse obj = JsonConvert.DeserializeObject<OnBotGetGuildMetaByGuestResponse>(message.Item2);
                return obj;
            }
            else
            {
                //失败
            }

            return null;
        }

        /// <summary>
        /// 等待返回数据
        /// </summary>
        /// <param name="echo"></param>
        /// <returns>返回code，以及message</returns>
        private async Task<(long, string)> WaitEcho(string echo, int waitMilliseconds = 10000)
        {
            string? message = "";
            long retcode = -1;
            int useMilliseconds = 0;

            await Task.Run(async () => {
                while (!_messageEcho.ContainsKey(echo))
                {
                    await Task.Delay(100);
                    useMilliseconds += 100;
                }

                if (_messageEcho.ContainsKey(echo))
                {
                    message = _messageEcho[echo];
                    _messageEcho.Remove(echo);

                    //TODO 需要保证返回的都是json
                    JsonObject node = (JsonObject)JsonNode.Parse(message);

                    if (node?.ContainsKey("retcode") == true)
                    {
                        retcode = (long)node["retcode"];
                    }
                }
                else
                {
                    //等待超时
                }

            });
            return (retcode, message);
        }
    }
}
