
using OpenAI;
using OpenAI.Chat;
using QinRSS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace QinRSS.Service
{
    internal class ChatGPTTranslatorManager
    {
        static ChatClient _client;

        const string kSystemMessage = "请把以下内容翻译为简体中文，不要解释：";

        static Dictionary<string, string> _cache = new Dictionary<string, string>();

        static ChatGPTTranslatorManager()
        {
            if (!string.IsNullOrEmpty(AppConfig.Data.OpenAIKey))
            {
                var model = string.IsNullOrWhiteSpace(AppConfig.Data.OpenAIAPIModel)
                    ? "gpt-4o-mini"
                    : AppConfig.Data.OpenAIAPIModel;

                _client = new ChatClient(model, new ApiKeyCredential(AppConfig.Data.OpenAIKey), BuildClientOptions());
            }
        }

        static OpenAIClientOptions BuildClientOptions()
        {
            var options = new OpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromSeconds(60)
            };

            if (!string.IsNullOrWhiteSpace(AppConfig.Data.OpenAIAPIBaseUri))
            {
                options.Endpoint = new Uri(AppConfig.Data.OpenAIAPIBaseUri);
            }

            if (!string.IsNullOrWhiteSpace(AppConfig.Data.OpenAIProxy))
            {
                var proxy = new WebProxy(AppConfig.Data.OpenAIProxy);
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };
                var httpClient = new HttpClient(handler);
                options.Transport = new HttpClientPipelineTransport(httpClient);
            }

            return options;
        }

        /// <summary>
        /// 完整调用一次翻译API
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static async Task<string> Translater(string s)
        {

            if (_client != null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return string.Empty;
                    }

                    var cacheKey = $"{kSystemMessage}{s}";
                    if (_cache.TryGetValue(cacheKey, out var r))
                    {
                        return r;
                    }

                    var completion = await _client.CompleteChatAsync(
                        new ChatMessage[]
                        {
                            new SystemChatMessage(kSystemMessage),
                            new UserChatMessage(s)
                        });

                    var text = string.Join(string.Empty, completion.Value.Content.Select(part => part.Text ?? string.Empty)).Trim();
                    _cache[cacheKey] = text;

                    return text;

                }
                catch (Exception ex)
                {
                    SimpleLogger.Instance.Error(ex.ToString());

                }
            }


            return string.Empty;
        }

    }

}
