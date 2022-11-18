using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service.OneBotModel
{
    

    public partial class OnBotGetGuildMetaByGuestResponse
    {
        [JsonProperty("data")]
        public OnBotGetGuildMetaByGuestGuildInfo Data { get; set; }

        [JsonProperty("echo")]
        public Guid Echo { get; set; }

        [JsonProperty("retcode")]
        public long Retcode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class OnBotGetGuildMetaByGuestGuildInfo
    {
        [JsonProperty("create_time")]
        public long CreateTime { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("guild_name")]
        public string GuildName { get; set; }

        [JsonProperty("guild_profile")]
        public string GuildProfile { get; set; }

        [JsonProperty("max_admin_count")]
        public long MaxAdminCount { get; set; }

        [JsonProperty("max_member_count")]
        public long MaxMemberCount { get; set; }

        [JsonProperty("max_robot_count")]
        public long MaxRobotCount { get; set; }

        [JsonProperty("member_count")]
        public long MemberCount { get; set; }

        [JsonProperty("owner_id")]
        public string OwnerId { get; set; }
    }
}
