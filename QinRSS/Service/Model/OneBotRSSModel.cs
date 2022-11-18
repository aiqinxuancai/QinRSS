using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service.Model
{
    /// <summary>
    /// 某个用户（Bot）的所有订阅信息
    /// </summary>
    public class OneBotRSSModel
    {
        public string SelfId { set; get; }

        public List<SubscriptionItemModel> AllSubscription { set; get; } = new List<SubscriptionItemModel>();
       
    }
}
