using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.MP.Enums;

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

        public long CreateTime { set; get; }

        /// <summary>
        /// 消息,用于去重
        /// </summary>
        public long MsgId { set; get; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public abstract WechatMPRequestMsgType MsgType { get; }

        public string RawXml { set; get; }

        /// <summary>
        /// 从xml中解析数据
        /// </summary>
        /// <param name="doc"></param>
        public virtual void LoadFromXml(XmlDocument doc)
        {
            var xmlNode = doc.GetElementsByTagName("xml")[0];

            this.AppId = xmlNode.GetFirstNodeByTagName("ToUserName").InnerText;
            this.FromUserOpenId=xmlNode.GetFirstNodeByTagName("FromUserName").InnerText;

            this.CreateTime = xmlNode.GetFirstNodeByTagName("CreateTime").InnerText.ToLong();

            //var dt =xmlNode.GetFirstNodeByTagName("CreateTime").InnerText.ToLong() ;

            this.ReceiveDt = (DateTime.Now.ToUniversalTime().Ticks - this.CreateTime) / 10000000;
            this.MsgId=xmlNode.GetFirstNodeByTagName("MsgId").InnerText.ToLong() ;

            LoadFromXmlInternal(xmlNode);
        }

        protected abstract void LoadFromXmlInternal(XmlNode xmlNode);
    }

    /// <summary>
    /// 文本消息
    /// </summary>
    public class WechatMPRequestText : WechatMPRequestBase
    {
        /// <summary>
        /// 文本消息内容
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        /// 用于在客服消息中,点击的菜单ID,,收到XML推送之后，开发者可以根据提取出来的bizmsgmenuid和Content识别出微信用户点击的是哪个菜单
        /// </summary>
        public string BizMsgMenuId { set; get; }

        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Text;
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.Content=xmlNode.GetFirstNodeByTagName("Content").InnerText;
            this.BizMsgMenuId = xmlNode.GetFirstElementsByTagName("bizmsgmenuid").InnerText;
        }
        
    }

    /// <summary>
    /// 图片消息
    /// </summary>
    public class WechatMPRequestImage : WechatMPRequestBase
    {
        /// <summary>
        /// 图片链接（由系统生成）
        /// </summary>
        public string PicUrl { set; get; }

        /// <summary>
        /// 图片消息媒体id，可以调用获取临时素材接口拉取数据
        /// </summary>
        public string MediaId { set; get; }

        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Image;

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.PicUrl=xmlNode.GetFirstNodeByTagName("PicUrl").InnerText;
            this.MediaId=xmlNode.GetFirstNodeByTagName("MediaId").InnerText;
        }
    }

    /// <summary>
    /// 语音消息,如果开启语音识别功能,可顺便获取语音分析后的文本
    /// </summary>
    public class WechatMPRequestVoice : WechatMPRequestBase
    {
        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Voice;

        /// <summary>
        /// 语音消息媒体id，可以调用获取临时素材接口拉取该媒体
        /// </summary>
        public string MediaId { set; get; }

        /// <summary>
        /// 语音文件格式,如amr等
        /// </summary>
        public string Format { set; get; }

        /// <summary>
        /// 开通语音识别后，用户每次发送语音给公众号时，微信会在推送的语音消息中，增加一个Recognition字段,并传入语音解析结果
        /// </summary>
        public string Recognition { set; get; }

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.Format=xmlNode.GetFirstNodeByTagName("Format").InnerText;
            this.MediaId=xmlNode.GetFirstNodeByTagName("MediaId").InnerText;
            this.Recognition=xmlNode.GetFirstNodeByTagName("Recognition")?.InnerText??String.Empty;
        }
    }

    /// <summary>
    /// 视频信息
    /// </summary>
    public class WechatMPRequestVideo : WechatMPRequestBase
    {
        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Video;

        /// <summary>
        /// 视频消息媒体id，可以调用获取临时素材接口拉取数据
        /// </summary>
        public string MediaId { set; get; }

        /// <summary>
        /// 视频消息缩略图的媒体id，可以调用获取临时素材接口拉取数据
        /// </summary>
        public string ThumbMediaId { set; get; }

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.ThumbMediaId=xmlNode.GetFirstNodeByTagName("ThumbMediaId").InnerText;
            this.MediaId=xmlNode.GetFirstNodeByTagName("MediaId").InnerText;
        }
    }

    /// <summary>
    /// 小视频消息
    /// </summary>
    public class WechatMPRequestShortVideo : WechatMPRequestBase
    {
        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.ShortVideo;

        /// <summary>
        /// 视频消息媒体id，可以调用获取临时素材接口拉取数据
        /// </summary>
        public string MediaId { set; get; }

        /// <summary>
        /// 视频消息缩略图的媒体id，可以调用获取临时素材接口拉取数据。
        /// </summary>
        public string ThumbMediaId { set; get; }

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.ThumbMediaId=xmlNode.GetFirstNodeByTagName("ThumbMediaId").InnerText;
            this.MediaId=xmlNode.GetFirstNodeByTagName("MediaId").InnerText;
        }
    }


    /// <summary>
    /// 用户主动上发的地理位置消息
    /// </summary>
    public class WechatMPRequestLocation : WechatMPRequestBase
    {
        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Location;

        /// <summary>
        /// 地理位置纬度
        /// </summary>
        public decimal Location_X { set; get; }

        /// <summary>
        /// 地理位置经度
        /// </summary>
        public decimal Location_Y { set; get; }

        /// <summary>
        /// 地图缩放大小
        /// </summary>
        public int Scale { set; get; }
        
        /// <summary>
        /// 地理位置信息
        /// </summary>
        public string Label { set; get; }
        

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.Location_X=xmlNode.GetFirstNodeByTagName("Location_X").InnerText.ToDecimal();
            this.Location_Y=xmlNode.GetFirstNodeByTagName("Location_Y").InnerText.ToDecimal();
            this.Scale=xmlNode.GetFirstNodeByTagName("Scale").InnerText.ToInt();
            this.Label=xmlNode.GetFirstNodeByTagName("Label").InnerText;
        }
    }

    /// <summary>
    /// 用户发送的链接消息
    /// </summary>
    public class WechatMPRequestLink : WechatMPRequestBase
    {
        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Link;

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 消息描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 消息链接
        /// </summary>
        public string Url { set; get; }
        

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.Title=xmlNode.GetFirstNodeByTagName("Title").InnerText;
            this.Description=xmlNode.GetFirstNodeByTagName("Description").InnerText;
            this.Url=xmlNode.GetFirstNodeByTagName("Url").InnerText;
        }
    }

    /// <summary>
    /// 事件消息基类
    /// </summary>
    public abstract class WechatMPRequestEventBase : WechatMPRequestBase
    {

        public override WechatMPRequestMsgType MsgType => WechatMPRequestMsgType.Event;

        /// <summary>
        /// 事件类型
        /// </summary>
        public abstract WechatMPRequestEventType EventType { get; }
        
    }

    /// <summary>
    /// 用于订阅事件,可以是搜索后订阅,也可以是扫码后订阅,如果是扫码后订阅的,则EventKey和Ticket有值
    /// </summary>
    public class WechatMPRequestSubscribe : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.EventKey=xmlNode.GetFirstNodeByTagName("EventKey").InnerText;
            this.Ticket=xmlNode.GetFirstNodeByTagName("Ticket").InnerText;
        }

        /// <summary>
        /// 事件KEY值，qrscene_为前缀，后面为二维码的参数值
        /// </summary>
        public string EventKey { set; get; }

        /// <summary>
        /// 二维码的ticket，可用来换取二维码图片
        /// </summary>
        public string Ticket { set; get; }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.Subscribe;
        
    }

    /// <summary>
    /// 用户扫码事件,已关注用户推送该事件
    /// </summary>
    public class WechatMPRequestScan : WechatMPRequestEventBase
    {
        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.ScanQrCode;

        /// <summary>
        /// 事件KEY值，是一个32位无符号整数，即创建二维码时的二维码scene_id
        /// </summary>
        public int EventKey { set; get; }

        /// <summary>
        /// 二维码的ticket，可用来换取二维码图片
        /// </summary>
        public string Ticket { set; get; }

        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.EventKey=xmlNode.GetFirstNodeByTagName("EventKey").InnerText.ToInt();
            this.Ticket=xmlNode.GetFirstNodeByTagName("Ticket").InnerText;
        }
    }

    /// <summary>
    /// 用户同意上报地理位置后，每次进入公众号会话时，都会在进入时上报地理位置，或在进入会话后每5秒上报一次地理位置
    /// </summary>
    public class WechatMPRequestReportLocation : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.Latitude=xmlNode.GetFirstNodeByTagName("Latitude").InnerText.ToDecimal();
            this.Longitude=xmlNode.GetFirstNodeByTagName("Longitude").InnerText.ToDecimal();
            this.Precision=xmlNode.GetFirstNodeByTagName("Precision").InnerText.ToDecimal();
        }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.Location;

        /// <summary>
        /// 地理位置纬度
        /// </summary>
        public decimal Latitude { set; get; }

        /// <summary>
        /// 地理位置经度
        /// </summary>
        public decimal Longitude { set; get; }

        /// <summary>
        /// 地理位置精度
        /// </summary>
        public decimal Precision { set; get; }
    }

    /// <summary>
    /// 点击菜单拉取消息时的事件推送
    /// </summary>
    public class WechatMPRequestClick : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.EventKey=xmlNode.GetFirstNodeByTagName("EventKey").InnerText;
        }

        /// <summary>
        /// 事件KEY值，与自定义菜单接口中KEY值对应
        /// </summary>
        public string EventKey { set; get; }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.Click;
    }

    /// <summary>
    /// 点击菜单跳转链接时的事件推送
    /// </summary>
    public class WechatMPRequestView : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            this.EventKey=xmlNode.GetFirstNodeByTagName("EventKey").InnerText;
        }

        /// <summary>
        /// 事件KEY值，设置的跳转URL
        /// </summary>
        public string EventKey { set; get; }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.View;
    }

    /// <summary>
    /// 模板消息发送结果
    /// </summary>
    public class WechatMPRequestTemplateMsgFinish : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            var status=xmlNode.GetFirstNodeByTagName("Status").InnerText;

            IsSuccess = status == "success";
            RawStatus = status;
        }

        /// <summary>
        /// 发送是否为成功,如果IsSuccess为false,可查看RawStatus属性
        /// </summary>
        public bool IsSuccess { set; get; }

        /// <summary>
        /// 发送结果的原始状态,如果IsSuccess为false,可查看该属性
        /// </summary>
        public string RawStatus { set; get; }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.TemplateSendJobFinish;
    }

    /// <summary>
    /// 订阅消息的订阅结果通知
    /// </summary>
    public class WechatMPRequestSubscribeMsgResult : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            var xmlNodelst = xmlNode.GetFirstNodeByTagName("SubscribeMsgPopupEvent");

            var lst = new List<SubscribeMsgPopupEventItem>(xmlNodelst.ChildNodes.Count);

            foreach (XmlNode node in xmlNodelst.ChildNodes)
            {
                lst.Add(new SubscribeMsgPopupEventItem()
                {
                    TemplateId = node.GetFirstNodeByTagName("TemplateId").InnerText,
                    IsAccept=node.GetFirstNodeByTagName("SubscribeStatusString").InnerText=="accept",
                    Scene=node.GetFirstNodeByTagName("PopupScene").InnerText
                });
            }

            Results = lst;
        }

        /// <summary>
        /// 订阅结果
        /// </summary>
        public IReadOnlyList<SubscribeMsgPopupEventItem> Results { set; get; }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.SubscribeMsgPopup;

        public class SubscribeMsgPopupEventItem
        {
            public string TemplateId { set; get; }

            /// <summary>
            /// 用户是否接受
            /// </summary>
            public bool IsAccept { set; get; }

            /// <summary>
            /// 订阅的场景值
            /// </summary>
            public string Scene { set; get; }
        }
    }

    /// <summary>
    /// 公众号订阅消息的发送结果
    /// </summary>
    public class WechatMPRequestSubscribeMsgSendResult : WechatMPRequestEventBase
    {
        protected override void LoadFromXmlInternal(XmlNode xmlNode)
        {
            var xmlNodelst = xmlNode.GetFirstNodeByTagName("SubscribeMsgSentEvent");

            var lst = new List<SubscribeMsgSendResultItem>(xmlNodelst.ChildNodes.Count);

            foreach (XmlNode node in xmlNodelst.ChildNodes)
            {
                var errorCode = node.GetFirstNodeByTagName("ErrorCode").ToInt();


                lst.Add(new SubscribeMsgSendResultItem()
                {
                    TemplateId = node.GetFirstNodeByTagName("TemplateId").InnerText,
                    IsSuccess = errorCode==0,
                    ErrorCode = errorCode,
                    MsgId = node.GetFirstNodeByTagName("MsgID").InnerText,
                    RawErrorStatus = node.GetFirstNodeByTagName("ErrorStatus").InnerText
                });
            }

            Results = lst;
        }

        /// <summary>
        /// 发送的结果
        /// </summary>
        public IReadOnlyList<SubscribeMsgSendResultItem> Results { set; get; }

        public class SubscribeMsgSendResultItem
        {
            public string TemplateId { set; get; }

            public int ErrorCode { set; get; }

            /// <summary>
            /// 发送是否成功,,如果不成功,请查看ErrorCode和RawErrorStatus
            /// </summary>
            public bool IsSuccess { set; get; }

            public string RawErrorStatus { set; get; }

            public string MsgId { set; get; }
        }

        public override WechatMPRequestEventType EventType => WechatMPRequestEventType.SubscribeMsgSendRsult;
    }

    public class WechatMPRequestCompare:IEqualityComparer<WechatMPRequestBase>
    {
        public bool Equals(WechatMPRequestBase x, WechatMPRequestBase y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.MsgId == y.MsgId;
        }

        public int GetHashCode(WechatMPRequestBase obj)
        {
            return obj.MsgId.GetHashCode();
        }
    }
}
