using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;

namespace Kugar.WechatSDK.MP.Entities
{
    [JsonConverter(typeof(PersonalizedMenuRuleJsonConverter))]
    public class PersonalizedMenuRule
    {
        /// <summary>
        /// 用户标签的id，可通过用户标签管理接口获取
        /// </summary>
        public int? TagId { set; get; }

        /// <summary>
        /// 性别：男（1）女（2），不填则不做匹配
        /// </summary>
        public int? Sex { set; get; }

        /// <summary>
        /// 客户端版本，当前只具体到系统型号：IOS(1), Android(2),Others(3)，不填则不做匹配
        /// </summary>
        public int? ClientPlatformType { set; get;}

        /// <summary>
        /// 国家信息，是用户在微信中设置的地区
        /// </summary>
        public string Country { set; get; }

        /// <summary>
        /// 省份信息，是用户在微信中设置的地区
        /// </summary>
        public string Province { set; get; }

        /// <summary>
        /// 城市信息，是用户在微信中设置的地区
        /// </summary>
        public string City { set; get; }

        /// <summary>
        /// 语言信息，是用户在微信中设置的语言
        /// </summary>
        public string Language { set; get; }
    }

    public class PersonalizedMenuRuleJsonConverter : JsonConverter<PersonalizedMenuRule>
    {
        public override void WriteJson(JsonWriter writer, PersonalizedMenuRule value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value.TagId >= 0) writer.WriteProperty("tag_id", value.TagId.Value);
            if (value.Sex.HasValue && (value.Sex==1||value.Sex==2)) writer.WriteProperty("sex", value.Sex.Value);
            if (value.ClientPlatformType >= 0 && (value.ClientPlatformType==1||value.ClientPlatformType==2 || value.ClientPlatformType==3)) writer.WriteProperty("client_platform_type", value.ClientPlatformType.Value);
            if (!string.IsNullOrWhiteSpace(value.Country)) writer.WriteProperty("country", value.Country);
            if (!string.IsNullOrWhiteSpace(value.Province)) writer.WriteProperty("province", value.Province);
            if (!string.IsNullOrWhiteSpace(value.City)) writer.WriteProperty("city", value.City);
            if (!string.IsNullOrWhiteSpace(value.Language)) writer.WriteProperty("language", value.Language);

            writer.WriteEndObject();
        }

        public override PersonalizedMenuRule ReadJson(JsonReader reader, Type objectType, PersonalizedMenuRule existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
