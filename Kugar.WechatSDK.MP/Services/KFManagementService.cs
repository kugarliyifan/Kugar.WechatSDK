using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Entities;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP.Services
{
    /// <summary>
    /// 客服相关模块
    /// </summary>
    public class KFManagementService:MPBaseService
    {
        public KFManagementService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 推送客服消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appId"></param>
        /// <param name="msg">客服消息实体</param>
        /// <returns></returns>
        public async Task<ResultReturn> SendMsg(string appId, CustomMessageBase msg)  
        {
            var data = await CommonApi.Post(appId,
                "/cgi-bin/message/custom/send?access_token=ACCESS_TOKEN",
                msg.ToJson()
            );

            return data;
        }

        /// <summary>
        /// 设置与用户聊天窗口中的输入中状态
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenId">用户OpenId</param>
        /// <param name="isTyping">是否是输入中</param>
        /// <returns></returns>
        public async Task<ResultReturn> SetTypingState(string appId, string userOpenId, bool isTyping)
        {
            var data = await CommonApi.Post(appId,
                "/cgi-bin/message/custom/send?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["touser"]=userOpenId,
                    ["command"]=isTyping?"Typing":"CancelTyping"
                }
            );

            return data;
        }
    }
}
