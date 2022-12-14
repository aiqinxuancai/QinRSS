using Newtonsoft.Json;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using HtmlAgilityPack;
using QinRSS.Service;
using System.Text;
using QinRSS.Service.Model;
using QinRSS.Utils;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using Flurl.Http;
using System.Web;

namespace QinRSS.Service
{
    public class SubscriptionManager
    {
        public delegate void SendSubscriptionHandler(string selfId, string guildId, string groupOrchannelId, DateTime time, string content, string url, List<string> imageUrls);

        private static SubscriptionManager instance = new SubscriptionManager();

        public static SubscriptionManager Instance
        {
            get
            {
                return instance;
            }
        }

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public List<OneBotRSSModel> SubscriptionModel => _subscriptionModel;

        private List<OneBotRSSModel> _subscriptionModel { get; set; } = new List<OneBotRSSModel>();

        private object _lock = new object();

        ~SubscriptionManager()
        {
            _tokenSource.Cancel();
        }

        /// <summary>
        /// 载入并启动订阅刷新
        /// </summary>
        public void Restart()
        {
            Load();
            Start();
        }

        public void Start()
        {
            if (_tokenSource != null) //TODO 停止任务
            {
                _tokenSource.Cancel();
            }

            var tokenSource = new CancellationTokenSource();
            _tokenSource = tokenSource;
            Task.Run(() => TimerFunc(tokenSource.Token));
        }

        public void Stop()
        {
            if (_tokenSource != null) //TODO 停止任务
            {
                _tokenSource.Cancel();
            }
        }

        private void TimerFunc(CancellationToken cancellationToken)
        {
            var c = cancellationToken;
            //首次启动10秒后进行第一次检查
            TaskHelper.Sleep(1000 * 10, 100, c);

            while (true)
            {
                SimpleLogger.Instance.Info($"TimerFunc订阅");
                if (c.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    foreach (var subscription in _subscriptionModel)
                    {
                        if (AppCache.Data.LastSentTime != null && 
                            (DateTime.Now - (DateTime)AppCache.Data.LastSentTime).TotalDays >= 1 &&
                            AppConfig.Data.NotSentAfterLongOffline)
                        {
                            CheckSubscription(subscription, true);
                        } 
                        else
                        {
                            CheckSubscription(subscription);
                        }
                        
                        AppCache.Data.LastSentTime = DateTime.Now;
                    }
                    
                }
                catch (Exception ex)
                {
                    SimpleLogger.Instance.Error(ex.ToString());
                }

                var runInterval = AppConfig.Data.RunInterval;
                if (runInterval <= 0)
                {
                    runInterval = 60;
                }

                TaskHelper.Sleep(1000 * runInterval, 100, cancellationToken);
            }
        }

        /// <summary>
        /// 通过网络获取订阅地址的Title
        /// </summary>
        /// <param name="url"></param>
        public string GetSubscriptionTitle(string url)
        {
            try
            {
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();
                SimpleLogger.Instance.Info($"获取订阅标题：{feed.Title.Text}");
                return feed.Title.Text;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error($"获取订阅标题失败");
                return "";
            }
        }

        /// <summary>
        /// 检查一次订阅
        /// </summary>
        private void CheckSubscription(OneBotRSSModel model, bool dontSend = false)
        {
            lock (_lock)
            {
                //缓存，避免同一个URL多次请求
                Dictionary<string, SyndicationFeed> urlCache = new Dictionary<string, SyndicationFeed>();


                SimpleLogger.Instance.Info($"检查{model.SelfId}订阅...");
                foreach (var subscription in model.AllSubscription)
                {
                    string url = subscription.Url;

                    if (!url.StartsWith("http"))
                    {
                        //拼接URL "https://rsshub.uneasy.win" 
                        url = UrlHelper.CombineUriToString(AppConfig.Data.RSSHubUrl, url); ;
                    }


                    SimpleLogger.Instance.Info($"检查订阅地址：{url}");

                    XmlReader reader;
                    SyndicationFeed feed;

                    if (urlCache.ContainsKey(url))
                    {
                        //从缓存取
                        feed = urlCache[url];
                    }
                    else
                    {
                        try
                        {
                            reader = XmlReader.Create(url);
                            feed = SyndicationFeed.Load(reader);
                            reader.Close();
                            urlCache.Add(url, feed);
                        }
                        catch (Exception e)
                        {
                            SimpleLogger.Instance.Error($"无法访问订阅：{url}");
                            continue;
                        }
                    }



                    subscription.TaskFullCount = feed.Items.Count();
                    subscription.Name = feed.Title.Text;

                    SimpleLogger.Instance.Info($"获得订阅标题：{subscription.Name} 订阅总任务数：{subscription.TaskFullCount}");

                    foreach (SyndicationItem item in feed.Items.Reverse())
                    {
                        string subject = item.Title.Text; // 摘要
                        string summary = item.Summary.Text; //完整 HTML格式

                        string itemUrl = item.Links?.FirstOrDefault().Uri.ToString();

                        var dateTime = item.PublishDate.ToLocalTime().DateTime;


                        var doc = new HtmlDocument();
                        doc.LoadHtml(summary);

                        string iStr = doc.DocumentNode.InnerText;

                        //var images = doc.DocumentNode.SelectNodes("/img");
                        var imageUrls = GetImageUrl(doc);

                        if (subscription.AlreadyAddedDownloadModel.Count == 0)
                        {
                            dontSend = true;
                        }

                        //如果没有加入过
                        if (!subscription.AlreadyAddedDownloadModel.Any(a => a.Url.Contains(itemUrl)))
                        {
                            try
                            {
                                if (dontSend)
                                {
                                    //不发送
                                    subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = itemUrl });
                                }
                                else
                                {
                                    subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = itemUrl });

                                    //发送订阅
                                    SendSubscription(model.SelfId, 
                                        subscription.GuildId, 
                                        subscription.GroupOrChannelId, 
                                        dateTime, 
                                        subscription.Name,
                                        iStr, 
                                        itemUrl, 
                                        imageUrls,
                                        subscription.Translate).Wait();

                                    Thread.Sleep(1000);
                                }

                            }
                            catch (Exception ex)
                            {
                                SimpleLogger.Instance.Error(ex.ToString());
                            }

                        }
                    }
                    //Save();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="selfId"></param>
        /// <param name="guildId">因为兼容频道，所以string</param>
        /// <param name="channelId">因为兼容频道，所以string</param>
        /// <param name="time"></param>
        /// <param name="content"></param>
        /// <param name="url"></param>
        /// <param name="imageUrls"></param>
        /// <returns></returns>
        private async Task SendSubscription(string selfId,
            string guildId,
            string channelId,
            DateTime time,
            string title,
            string content,
            string url,
            IEnumerable<string> imageUrls,
            bool translate)
        {
            SimpleLogger.Instance.Info($"开始发送订阅{channelId}");
            SimpleLogger.Instance.Info($"{time} {url}");
            SimpleLogger.Instance.Info($"{content}"); //TODO 支持翻译deepl

            var urlBase64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(url));

            string sendText = "";

            if (translate)
            {
                var translates = await TranslatorManager.Translater(content.Replace("※", ""));
                if (translates != null)
                {
                    var translateContent = translates;

                    sendText += $"{title}\n原文：\n{content}\n译文：\n{translateContent}\n";
                }
                else
                {
                    sendText = $"{title}\n{content}\n";
                }
            }
            else
            {
                sendText = $"{title}\n{content}\n";
            }

            //图片CQ码
            foreach (var imageUrl in imageUrls)
            {
                //图片下载逻辑，图片使用代理下载逻辑
                //imageUrl.DownloadFileAsync(AppContext.BaseDirectory)
                sendText += $"\n[CQ:image,file={imageUrl}]";
            }

            sendText += $"\n更新时间：{time.ToString("yyyy-MM-dd HH:mm:ss")}\n链接：{url}";

            SimpleLogger.Instance.Error($"发送订阅：{selfId}, {channelId}, {sendText}");
            sendText = HttpUtility.HtmlDecode(sendText);

            if (AppConfig.Data.SelfDownloadImage)
            {
                sendText = await DownloadHelper.MessageImageUrlToBase64(sendText);
            }
            

            if (string.IsNullOrEmpty(guildId))
            {
                //qq群
                await WebSocketManager.Instance.SendGroupMessage(selfId, channelId, sendText);
            }
            else
            {
                //频道
                await WebSocketManager.Instance.SendChannelMessage(selfId, guildId, channelId, sendText);
            }
        }


        public IEnumerable<string> GetImageUrl(HtmlDocument doc)
        {
            //var list = new List<string>();
            var images = doc.DocumentNode.SelectNodes("/img");
            var videos = doc.DocumentNode.SelectNodes("/video");
            
            if (images != null)
            {
                foreach (var image in images)
                {
                    var src = image.Attributes["src"]?.Value;
                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        //list.Add(src);
                        yield return src;
                    }
                }
            }



            if (videos != null)
            {
                foreach (var video in videos)
                {
                    //视频缩略图
                    if (video.Attributes.Contains("poster"))
                    {
                        var poster = video.Attributes["poster"]?.Value;
                        if (!string.IsNullOrWhiteSpace(poster))
                        {
                            yield return poster;
                        }
                    }
                }
            }
            yield break;
        }


        public void Load()
        {
            lock (_lock)
            {
                string fileName = Path.Combine(AppContext.BaseDirectory, "Subscription.json");
                SimpleLogger.Instance.Error($"准备载入{fileName}");
                if (File.Exists(fileName))
                {
                    try
                    {
                        _subscriptionModel.Clear();
                        List<OneBotRSSModel> subscriptionModel = JsonConvert.DeserializeObject<List<OneBotRSSModel>>(File.ReadAllText(fileName));


                        _subscriptionModel = subscriptionModel;
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Instance.Error($"载入失败");
                    }


                }

            }
        }


        public void Save()
        {
            lock (_lock)
            {
                Debug.WriteLine("保存订阅");
                string fileName = Path.Combine(AppContext.BaseDirectory, "Subscription.json");
                var content = JsonConvert.SerializeObject(_subscriptionModel);
                File.WriteAllText(fileName, content);
            }

        }

        //存储订阅，读取加载订阅

        public bool Add(string selfId, string guildId, string channelId, string name, string url, bool translate)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception($"添加失败，名字为空");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception($"添加失败，url为空");
            }

            List<SubscriptionItemModel> list = new();
            OneBotRSSModel oneBotRSSModel = null;
            if (_subscriptionModel.Any(a => a.SelfId == selfId))
            {
                oneBotRSSModel = _subscriptionModel.FirstOrDefault(a => a.SelfId == selfId);
                list = oneBotRSSModel.AllSubscription;
            }
            else
            {
                oneBotRSSModel = new OneBotRSSModel() { AllSubscription = list, SelfId = selfId };
                _subscriptionModel.Add(oneBotRSSModel);
            }

            if (list.ToList().Find( a => { return a.Url == url && a.GroupOrChannelId == guildId && a.GuildId == guildId; }) != null)
            {
                //找到了存在相同
                SimpleLogger.Instance.Error($"添加失败，重复的订阅");
                throw new Exception($"添加失败，重复的订阅");
            }

            SubscriptionItemModel model = new SubscriptionItemModel();
            model.CustomName = name;
            model.Url = url;
            model.GroupOrChannelId = channelId;
            model.GuildId = guildId;
            model.Translate = translate;

            SimpleLogger.Instance.Error($"添加订阅：{model.Url}");

            if (list.Count >= 50) //避免长期运行后订阅文件超大
            {
                list.RemoveAt(0);
            }
            list.Add(model);
            Save();
            Task.Run(() => {
                CheckSubscription(oneBotRSSModel, true);
            });
            
            return true;
        }

        public void Remove(string selfId, string guildId, string channelId, string name)
        {
            List<SubscriptionItemModel> list = new();
            OneBotRSSModel oneBotRSSModel = null;
            if (_subscriptionModel.Any(a => a.SelfId == selfId))
            {
                oneBotRSSModel = _subscriptionModel.FirstOrDefault(a => a.SelfId == selfId);
                list = oneBotRSSModel.AllSubscription;
            }
            else
            {
                oneBotRSSModel = new OneBotRSSModel() { AllSubscription = list, SelfId = selfId };
                _subscriptionModel.Add(oneBotRSSModel);
            }


            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].CustomName == name && guildId == list[i].GuildId && list[i].GroupOrChannelId == channelId)
                {
                    list.Remove(list[i]);
                    break;
                }
            }

            Save();
        }


        public void Clear(string selfId, string guildId, string channelId)
        {
            List<SubscriptionItemModel> list = new();
            OneBotRSSModel oneBotRSSModel = null;
            if (_subscriptionModel.Any(a => a.SelfId == selfId))
            {
                oneBotRSSModel = _subscriptionModel.FirstOrDefault(a => a.SelfId == selfId);
                list = oneBotRSSModel.AllSubscription;
            }
            else
            {
                oneBotRSSModel = new OneBotRSSModel() { AllSubscription = list, SelfId = selfId };
                _subscriptionModel.Add(oneBotRSSModel);
            }


            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (guildId == list[i].GuildId && list[i].GroupOrChannelId == channelId)
                {
                    list.Remove(list[i]);
                }
            }

            Save();
        }


        public string List(string selfId, string guildId, string channelId)
        {
            List<SubscriptionItemModel> list = new();
            OneBotRSSModel oneBotRSSModel = null;
            if (_subscriptionModel.Any(a => a.SelfId == selfId))
            {
                oneBotRSSModel = _subscriptionModel.FirstOrDefault(a => a.SelfId == selfId);
                list = oneBotRSSModel.AllSubscription;
            }
            else
            {
                oneBotRSSModel = new OneBotRSSModel() { AllSubscription = list, SelfId = selfId };
                _subscriptionModel.Add(oneBotRSSModel);
            }

            var ret = string.Empty;
            foreach (var item in list)
            {
                if (guildId == item.GuildId && item.GroupOrChannelId == channelId)
                {
                    ret += $"{item.CustomName} [{item.Name}] {item.Url}\n";
                }
            }

            return ret;
        }

    }
}
