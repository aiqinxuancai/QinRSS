using Flurl.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NitterAPI.Services
{

    public class Tweet
    {
        public string link { get; set; }
        public DateTime time { get; set; }
        public string content { get; set; }
        public List<string> images { get; set; }
    }


    internal class NitterManager
    {

        const string kBaseUrl = "https://nitter.net";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static JsonArray GetNitter(string name)
        {
            var htmlString = $"{kBaseUrl}/{name}"
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36")
            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7")
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .WithHeader("Accept-Language", "zh-CN,zh;q=0.9,ja;q=0.8")
            .GetStringAsync().Result;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString); // htmlString 是你的HTML字符串

            // 查找所有的timeline-item元素
            var timelineItems = doc.DocumentNode.SelectNodes("//div[@class='timeline-item ']");

            var title = doc.DocumentNode.SelectSingleNode("//title").InnerHtml.Replace(" | nitter", "");


            JsonArray jsonArray = new JsonArray();
            foreach (var item in timelineItems)
            {
                JsonObject json = new JsonObject();

                // 提取tweet-link
                var tweetLink = item.SelectSingleNode(".//a[@class='tweet-link']").GetAttributeValue("href", string.Empty);
                //Console.WriteLine("Tweet link: " + tweetLink);
                json["link"] = tweetLink;

                var tweetPinned = item.SelectSingleNode(".//div[@class='pinned']");
                if (tweetPinned != null)
                {
                    continue;
                }

                // 提取tweet-data
                var tweetDate = item.SelectSingleNode(".//span[@class='tweet-date']/a").GetAttributeValue("title", string.Empty);
                var format = "MMM d, yyyy · h:mm tt 'UTC'";
                var provider = CultureInfo.InvariantCulture;
                var date = DateTime.ParseExact(tweetDate, format, provider, DateTimeStyles.AssumeUniversal);

                //Console.WriteLine("Tweet Date: " + tweetDate + " " + date);

                json["time"] = date;


                // 提取tweet-content media-body的内容
                var tweetContent = item.SelectSingleNode(".//div[@class='tweet-content media-body']").InnerText.Trim();
                //Console.WriteLine("Tweet content: " + tweetContent);

                json["contnent"] = tweetContent;


                // 提取attachment image中的a标签的图片URL
                var images = item.SelectNodes(".//div[@class='attachment image']/a");
                if (images != null)
                {
                    JsonArray imagesArray = new JsonArray();
                    foreach (var image in images)
                    {
                        var imageUrl = image?.GetAttributeValue("href", string.Empty);
                        //Console.WriteLine("Image URL: " + imageUrl);
                        JsonValue stringNode = JsonValue.Create($"{kBaseUrl}{imageUrl}");
                        imagesArray.AsArray().Add(stringNode);
                    }
                    json["images"] = imagesArray;
                }
                jsonArray.Add(json);


            }
            //Console.WriteLine(jsonArray);
            //Console.ReadLine();
            return jsonArray;
        }
    }
}
