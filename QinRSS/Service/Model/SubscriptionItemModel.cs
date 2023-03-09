using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace QinRSS.Service.Model
{
    public enum SubscriptionType
    {
        Normal, //普通群
        Channel //频道
    }


    public partial class SubscriptionItemModel 
    {
        /// <summary>
        /// 订阅地址
        /// </summary>
        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 群号或者频道号
        /// </summary>
        [JsonProperty("GroupOrChannelId")]
        public string GroupOrChannelId { get; set; }

        /// <summary>
        /// 频道ID（仅Channel有）
        /// </summary>
        [JsonProperty("GuildId")] 
        public string GuildId { get; set; }

        /// <summary>
        /// 使用翻译
        /// </summary>
        [JsonProperty("Translate")]
        public bool Translate { get; set; }


        /// <summary>
        /// 不保留原文，只使用翻译
        /// </summary>
        [JsonProperty("TranslateOnly")]
        public bool TranslateOnly { get; set; }

        [JsonProperty("SubscriptionType")]
        public SubscriptionType SubscriptionType { get; set; }

        /// <summary>
        /// 自定义名字 考虑移除？
        /// </summary>
        [JsonProperty("CustomName")]
        public string CustomName { get; set; }

        /// <summary>
        /// 任务总数
        /// </summary>
        [JsonProperty("TaskFullCount")]
        public int TaskFullCount { get; set; }

        /// <summary>
        /// 已经发送过的
        /// </summary>
        //[JsonIgnore]
        [JsonProperty("AlreadyAddedDownloadModel")]
        public List<SubscriptionSubTaskModel> AlreadyAddedDownloadModel { get; set; } = new List<SubscriptionSubTaskModel> { };
    }

    public partial class SubscriptionSubTaskModel 
    {
        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public partial class SubscriptionItemModel
    {
        public static SubscriptionItemModel[] FromJson(string json) => JsonConvert.DeserializeObject<SubscriptionItemModel[]>(json, SubscriptionItemModelConverter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SubscriptionItemModel[] self) => JsonConvert.SerializeObject(self, SubscriptionItemModelConverter.Settings);
    }

    internal static class SubscriptionItemModelConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
