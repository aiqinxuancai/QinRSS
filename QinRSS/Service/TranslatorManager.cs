
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
    internal class TranslatorManager
    {


        static TranslatorManager()
        {
        }

        private static string ROT13(string p)
        {
            var t = "NOPQRSTUVWXYZABCDEFGHIJKLMnopqrstuvwxyzabcdefghijklm";
            var o = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string s = "";
            foreach (var i in p)
            {
                var index = o.IndexOf(i);
                if (index > -1)
                {
                    s += t[index];
                }
                else
                {
                    s += i;
                }
            }
            return s;
        }

        private static string CaiyunDecode(string s)
        {
            s = ROT13(s);
            var s2 = Base64Decode(s);
            return s2;

        }


        private static string Base64Decode(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }



        private static async Task<(string, string)> GenerateJWT()
        {
            var url = "https://api.interpreter.caiyunai.com/v1/user/jwt/generate";

            var browserId = HashHelper.GetMD5(Guid.NewGuid().ToString()).ToLower();

            JsonObject json = new JsonObject();
            json["browser_id"] = browserId;

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                {"X-Authorization", "token:qgemv4jr1y38jyq6vhvi" },
                {"Content-Type", "application/json;charset=UTF-8" },
                {"app-name", "xy" },
            };

            var r = await url.WithHeaders(headers).PostStringAsync(json.ToJsonString());

            var rj = await r.GetStringAsync();
            JsonNode j = JsonNode.Parse(rj);

            return ((string)j["jwt"], browserId);
        }



        private static async Task<CaiyunResult> Translater(string s, string jwt, string browserId)
        {
            var url = "https://api.interpreter.caiyunai.com/v1/translator";

            JsonObject json = new JsonObject();
            json["source"] = s;
            json["trans_type"] = "auto2zh";
            json["request_id"] = "web_fanyi";
            json["media"] = "text";
            json["os_type"] = "web";
            json["dict"] = true;
            json["cached"] = true;
            json["detect"] = true;
            json["replaced"] = true;
            json["browser_id"] = browserId;

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                {"X-Authorization", "token:qgemv4jr1y38jyq6vhvi" },
                {"Content-Type", "application/json;charset=UTF-8" },
                {"app-name", "xy" },
                {"T-Authorization", jwt },
            };

            var sendString = json.ToJsonString();

            var r = await url.WithHeaders(headers).PostStringAsync(sendString);

            var rj = await r.GetStringAsync();

            Debug.WriteLine(rj);
            CaiyunResult j = JsonConvert.DeserializeObject<CaiyunResult>(rj);

            return j;
        }

        /// <summary>
        /// 完整调用一次翻译API
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static async Task<string> Translater(string s)
        {
            List<string> list = new List<string>();

            try
            {
                var j = await GenerateJWT();
                var ret = await Translater(s, j.Item1, j.Item2);


                return CaiyunDecode(ret.Target);
       

            }
            catch(Exception ex) 
            {
                SimpleLogger.Instance.Error(ex.ToString());
            
            }

            return string.Empty;
        }

    }


    public partial class CaiyunResult
    {
        [JsonProperty("src_tgt")]
        public string[] SrcTgt { get; set; }
        
        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("rc")]
        public double Rc { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
