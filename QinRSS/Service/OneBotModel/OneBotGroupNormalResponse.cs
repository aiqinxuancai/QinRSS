using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service.OneBotModel
{
    /// <summary>
    /// QQ群消息
    /// </summary>
    public partial class OneBotGroupNormalResponse
    {
        [JsonProperty("post_type")]
        public string PostType { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("self_id")]
        public long SelfId { get; set; }

        [JsonProperty("sub_type")]
        public string SubType { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("raw_message")]
        public string RawMessage { get; set; }

        [JsonProperty("sender")]
        public OneBotGroupNormalSender Sender { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("message_id")]
        public long MessageId { get; set; }

        [JsonProperty("anonymous")]
        public object Anonymous { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("font")]
        public long Font { get; set; }

        [JsonProperty("message_seq")]
        public long MessageSeq { get; set; }
    }

    public partial class OneBotGroupNormalSender
    {
        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }

}
