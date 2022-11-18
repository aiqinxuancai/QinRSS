using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service.OneBotModel
{
    public partial class OnBotGetGuildListResponse
    {
        [JsonProperty("data")]
        public GuildInfo[] Data { get; set; }

        [JsonProperty("echo")]
        public Guid Echo { get; set; }

        [JsonProperty("retcode")]
        public long Retcode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class GuildInfo
    {
        [JsonProperty("guild_display_id")]
        public string GuildDisplayId { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("guild_name")]
        public string GuildName { get; set; }
    }
}
