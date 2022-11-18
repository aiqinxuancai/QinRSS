using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QinRSS.Service.OneBotModel
{
    /// <summary>
    /// QQ频道消息
    /// </summary>
    public partial class OneBotGroupChannelResponse
    {
        [JsonProperty("post_type")]
        public string PostType { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("self_id")]
        public string SelfId { get; set; }

        [JsonProperty("sub_type")]
        public string SubType { get; set; }

        [JsonProperty("sender")]
        public OneBotGroupChannelSender Sender { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("self_tiny_id")]
        public string SelfTinyId { get; set; }
    }

    public partial class OneBotGroupChannelSender
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("tiny_id")]
        public string TinyId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }





}


