using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service.OneBotModel
{
    public class OneBotGroupMemberInfoResponse
    {
        [JsonProperty("data")]
        public OneBotGroupMember Data { get; set; }

        [JsonProperty("echo")]
        public Guid Echo { get; set; }

        [JsonProperty("retcode")]
        public long Retcode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class OneBotGroupMember
    {
        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("card_changeable")]
        public bool CardChangeable { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("join_time")]
        public long JoinTime { get; set; }

        [JsonProperty("last_sent_time")]
        public long LastSentTime { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; }

        [JsonProperty("shut_up_timestamp")]
        public long ShutUpTimestamp { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_expire_time")]
        public long TitleExpireTime { get; set; }

        [JsonProperty("unfriendly")]
        public bool Unfriendly { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }
}




