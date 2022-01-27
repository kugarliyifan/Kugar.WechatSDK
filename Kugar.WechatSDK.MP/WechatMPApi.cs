using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kugar.WechatSDK.MP.Services;

namespace Kugar.WechatSDK.MP
{
    public interface IWechatMPApi
    {
        /// <summary>
        /// 公众号菜单模块
        /// </summary>
        IMenuService Menu { get; }

        /// <summary>
        /// 公众号第三方授权模块
        /// </summary>
        IOAuthService OAuth { get; }

        /// <summary>
        /// 公众号网页JS功能模块
        /// </summary>
        IUIService JsUI { get; }

        /// <summary>
        /// 公众号消息模块
        /// </summary>
        IMessageService Message { get; }

        /// <summary>
        /// 素材功能模块
        /// </summary>
        IMaterialService Material { get; }

        /// <summary>
        /// 群发消息模块
        /// </summary>
        IBrocastMsgService BrocastMsg { get; }

        /// <summary>
        /// 模板消息模块
        /// </summary>
        ITemplateMsgService TemplateMsg { get; }

        /// <summary>
        /// 公众号订阅消息模块
        /// </summary>
        ISubscriptionMsgService SubscriptionMsg { get; }

        /// <summary>
        /// 用户管理功能模块(包含标签管理)
        /// </summary>
        IUserManagementService UserManagement { get; }

        /// <summary>
        /// 推广二维码相关模块
        /// </summary>
        IQrCodeService QrCode { get; }

        /// <summary>
        /// 推广相关的短连接及短key相关模块
        /// </summary>
        IUrlService Url { get; }

        /// <summary>
        /// 客服管理模块
        /// </summary>
        KFManagementService KFManagement { get; }
    }

    public class WechatMPApi : IWechatMPApi
    {
        public WechatMPApi(
            IMenuService menu,
            IOAuthService oauth,
            IUIService ui,
            IMessageService message,
            IMaterialService material,
            IBrocastMsgService brocastMsg,
            ITemplateMsgService templateMsg,
            ISubscriptionMsgService subscriptionMsg,
            IUserManagementService userManagement,
            IQrCodeService qrCode,
            IUrlService url,
            KFManagementService kfManagement
            )
        {
            //Debugger.Break();
            Menu = menu;
            OAuth = oauth;
            JsUI = ui;
            Message = message;
            Material = material;
            BrocastMsg = brocastMsg;
            TemplateMsg = templateMsg;
            SubscriptionMsg = subscriptionMsg;
            UserManagement = userManagement;
            QrCode = qrCode;
            Url = url;
            KFManagement = kfManagement;
        }

        public IMenuService Menu { get; }

        public IOAuthService OAuth { get; }

        public IUIService JsUI { get; }
        public IMessageService Message { get; }
        public IMaterialService Material { get; }
        public IBrocastMsgService BrocastMsg { get; }
        public ITemplateMsgService TemplateMsg { get; }
        public ISubscriptionMsgService SubscriptionMsg { get; }
        public IUserManagementService UserManagement { get; }
        public IQrCodeService QrCode { get; }
        public IUrlService Url { get; }
        public KFManagementService KFManagement { get; }
    }
}
