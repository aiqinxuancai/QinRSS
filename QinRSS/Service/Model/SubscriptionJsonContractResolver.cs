using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace QinRSS.Service.Model
{

    class SubscriptionJsonContractResolver : DefaultContractResolver
    {
        public bool includeClearTask { get; set; }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "AlreadyAddedDownloadModel" && !includeClearTask)
            {
                property.ShouldSerialize = instance => false;
            }
            //if (property.PropertyName == "Name" && !includeClearTask)
            //{
            //    property.ShouldSerialize = instance => false;
            //}
            return property;
        }
    }

}
