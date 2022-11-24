using QinRSS.Service;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QinRSS.Utils
{


    internal class DownloadHelper
    {
        static string _cachePath = Path.Combine(AppContext.BaseDirectory, "cache");

        public static async Task<string> MessageImageUrlToBase64(string message)
        {
            var regex = new Regex(@"\[CQ:image,file=(.*)\]", RegexOptions.Multiline);

            var matchs = regex.Matches(message);
            var tasks = new List<Task>();
            object l = new object();
            foreach (Match match in matchs)
            {
                if (match.Success)
                {
                    try 
                    {
                        tasks.Add(Task.Run(async () => {

                            var url = match.Groups[1].ToString();
                            if (!url.StartsWith("http"))
                            {
                                return;
                            }
                            Uri uri = new Uri(url);
                            var base64 = await GetImageBase64(url);

                            if (!string.IsNullOrEmpty(base64))
                            {
                                lock (l)
                                {
                                    message = message.Replace(match.Groups[0].ToString(), $"[CQ:image,file=base64://{base64}]");
                                }
                            }

                        })) ;
                    }
                    catch (Exception ex)
                    {
                    }


                }
            }
            //await tasks.ToArray();

            var taskWhenAll = Task.WhenAll(tasks.ToArray());
            await taskWhenAll.WaitAsync(TimeSpan.FromSeconds(60));

            //Task.WaitAll(tasks.ToArray());

            return message;

        }


        public static async Task<string> GetImageBase64(string url)
        {
            try
            {
                if (!Directory.Exists(_cachePath))
                {
                    Directory.CreateDirectory(_cachePath);
                }

                //检查图片缓存？

                var proxyHttpClientHandler = new HttpClientHandler();
                if (!string.IsNullOrWhiteSpace(AppConfig.Data.ImageProxy))
                {
                    var webProxy = new WebProxy(new Uri(AppConfig.Data.ImageProxy), BypassOnLocal: false);

                    proxyHttpClientHandler = new HttpClientHandler
                    {
                        Proxy = webProxy,
                        UseProxy = true,
                    };
                }

                HttpClient client = new HttpClient(proxyHttpClientHandler);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsByteArrayAsync();

                if (resp != null)
                {
                    //Uri uri = new Uri(url);
                    //var fileName = Guid.NewGuid().ToString() + ".jpg"; //Path.GetFileName(url))
                    //var fileCachePath = Path.Combine(_cachePath, fileName);
                    //Console.WriteLine($"缓存文件：{fileCachePath}");
                    //File.WriteAllBytes(fileCachePath, resp);
                    return Convert.ToBase64String(resp);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Instance.Error(ex.ToString());
            }

            return string.Empty;
        }
    }
}
