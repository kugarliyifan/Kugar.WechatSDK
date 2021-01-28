using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Kugar.WechatSDK.MP.Entities
{
    /// <summary>
    /// 微信推送的消息基类
    /// </summary>
    public abstract class WechatMPRequestBase
    {
        /// <summary>
        /// 消息发送者OpenID
        /// </summary>
        public string FromUserOpenId { set; get; }

        /// <summary>
        /// 接收到消息的AppID
        /// </summary>
        public string AppId { set;get; }

        /// <summary>
        /// 接收时的时间
        /// </summary>
        public long ReceiveDt { set; get; }

        /// <summary>
        /// 消息,用于去重
        /// </summary>
        public long MsgId { set; get; }

        /// <summary>
        /// 从xml中解析数据
        /// </summary>
        /// <param name="doc"></param>
        public abstract void LoadFromXml(XmlDocument doc);
    }

    public class WechatMPRequestText : WechatMPResponseBase
    {
        /// <summary>
        /// 文本消息内容
        /// </summary>
        public string Content { set; get; }

        public override string ToXml()
        {
            throw new NotImplementedException();
        }
    }
}
