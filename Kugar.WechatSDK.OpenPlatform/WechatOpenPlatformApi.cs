using System;
using Kugar.WechatSDK.OpenPlatform.Services;

namespace Kugar.WechatSDK.OpenPlatform
{
    /// <summary>
    /// 微信第三方开放平台Api
    /// </summary>
    public interface IWechatOpenPlatformApi
    {
        /// <summary>
        /// 微信授权相关功能模块
        /// </summary>
        IOAuthService OAuth { get; }

        /// <summary>
        /// 一次性订阅消息模块
        /// </summary>
        ISubscriptionMsgService SubscriptionMsg { get; }
    }

    /// <summary>
    /// 微信第三方开放平台Api
    /// </summary>
    public class WechatOpenPlatformApi : IWechatOpenPlatformApi
    {
        public WechatOpenPlatformApi(IOAuthService oauth, ISubscriptionMsgService subscriptionMsg)
        {
            OAuth = oauth;
            SubscriptionMsg = subscriptionMsg;
        }

        /// <summary>
        /// 微信授权相关功能模块
        /// </summary>
        public IOAuthService OAuth { get; }
        
        /// <summary>
        /// 一次性订阅消息模块
        /// </summary>
        public ISubscriptionMsgService SubscriptionMsg { get; }
    }
}
