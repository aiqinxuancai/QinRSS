
using ChatGPTSharp;
using Flurl.Http;
using Newtonsoft.Json;
using QinRSS.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace QinRSS.Service
{
    internal class ChatGPTTranslatorManager
    {
        static ChatGPTClient _client;

        static string _lastConversationId = string.Empty;

        static string _lastParentMessageId = string.Empty;


        const string kSystemMessage = "你将作为翻译官，请把以下内容翻译为中文，不要添加解释：\n";


        static Dictionary<string, string> _cache = new Dictionary<string, string>();

        static ChatGPTTranslatorManager()
        {
            if (!string.IsNullOrEmpty(AppConfig.Data.OpenAIKey))
            {
                _client = new ChatGPTClient(AppConfig.Data.OpenAIKey, timeoutSeconds: 60, proxyUri: AppConfig.Data.OpenAIProxy);
                if (!string.IsNullOrWhiteSpace(AppConfig.Data.OpenAIAPIBaseUri))
                {
                    _client.OpenAIAPIBaseUri = AppConfig.Data.OpenAIAPIBaseUri;
                }
                
            }
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
                    s = $"{kSystemMessage}{s}";

                    if (_cache.TryGetValue(s, out var r))
                    {
                        return r;
                    }


                    var result = await _client.SendMessage(s, _lastConversationId, _lastParentMessageId, ChatGPTSharp.Model.SendSystemType.None);

                    _lastParentMessageId = result.ConversationId;
                    _lastConversationId = result.MessageId;


                    _cache[s] = string.IsNullOrEmpty(result.Response) ? "" : result.Response;

                    return result.Response;

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
