using System;
using Kugar.WechatSDK.MiniProgram;

namespace Kugar.WechatSDK
{
    public interface IMiniProgramApi
    {
        /// <summary>
        /// 订阅消息模块
        /// </summary>
        ISubscribeMessage SubscribeMessage { get; }

        /// <summary>
        /// 小程序码/二维码功能
        /// </summary>
        IQrCode QrCode { get; }

        /// <summary>
        /// 账号相关功能
        /// </summary>
        ISNS OAuth { get; }

        /// <summary>
        /// UrlScheme模块功能,用于H5跳入小程序
        /// </summary>
        IUrlScheme UrlScheme { get; }
    }

    /// <summary>
    /// 小程序服务器端接口
    /// </summary>
    public class MiniProgramApi : IMiniProgramApi
    {
        internal MiniProgramApi(
            ISubscribeMessage subscribe,
            IQrCode qrcode,
            ISNS sns,
            IUrlScheme urlScheme
            )
        {
            SubscribeMessage = subscribe;
            QrCode = qrcode;
            OAuth = sns;
            UrlScheme = urlScheme;
        }

        /// <summary>
        /// 订阅消息模块
        /// </summary>
        public ISubscribeMessage SubscribeMessage { get; }

        /// <summary>
        /// 小程序码/二维码功能
        /// </summary>
        public IQrCode QrCode { get; }

        /// <summary>
        /// 账号相关功能
        /// </summary>
        public ISNS OAuth { get; }

        /// <summary>
        /// UrlScheme模块功能,用于H5跳入小程序
        /// </summary>
        public IUrlScheme UrlScheme { get; }
    }

}
