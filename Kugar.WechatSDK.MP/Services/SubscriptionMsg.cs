using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    public interface ISubscriptionMsgService
    {
        /// <summary>
        /// 推送订阅消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="toUserOpenId"></param>
        /// <param name="template_id"></param>
        /// <param name="data"></param>
        /// <param name="gotoUrl"></param>
        /// <param name="miniProgramAppId"></param>
        /// <param name="miniProgramPath"></param>
        /// <returns></returns>
        Task<ResultReturn> SendMsg(string appId, 
            string toUserOpenId, 
            string template_id,
            (string key, string value)[] data,
            string gotoUrl = "",
            string miniProgramAppId = "",
            string miniProgramPath = "");
    }

    public class SubscriptionMsgService:MPBaseService, ISubscriptionMsgService
    {
        public SubscriptionMsgService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 推送订阅消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="toUserOpenId"></param>
        /// <param name="template_id"></param>
        /// <param name="data"></param>
        /// <param name="gotoUrl"></param>
        /// <param name="miniProgramAppId"></param>
        /// <param name="miniProgramPath"></param>
        /// <returns></returns>
        public async Task<ResultReturn> SendMsg(string appId, 
            string toUserOpenId, 
            string template_id,
            (string key, string value)[] data,
            string gotoUrl = "",
            string miniProgramAppId = "",
            string miniProgramPath = "")
        {
            var args = new JObject()
            {
                ["touser"] = toUserOpenId,
                ["template_id"] = template_id
            };

            var dataJson = new JObject();

            foreach (var item in data)
            {
                dataJson.Add(item.key,new JObject()
                {
                    ["value"]=item.value
                });
            }

            args.Add("data",dataJson);

            args.AddPropertyIf(!string.IsNullOrWhiteSpace(gotoUrl), "page", gotoUrl);
            args.AddPropertyIf(!string.IsNullOrWhiteSpace(miniProgramAppId), "miniprogram", new JObject()
            {
                ["appid"] = miniProgramAppId,
                ["pagepath"] = miniProgramPath
            });

            var ret =await CommonApi.Post(appId, "/cgi-bin/message/subscribe/bizsend?access_token=ACCESS_TOKEN", args);

            return ret;
        }
    }
}
