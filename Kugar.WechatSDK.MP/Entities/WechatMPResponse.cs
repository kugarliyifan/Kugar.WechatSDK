using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.MP.Enums;

namespace Kugar.WechatSDK.MP.Entities
{
    /// <summary>
    /// 微信回复消息的基类
    /// </summary>
    public abstract class WechatMPResponseBase
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public abstract WechatResposeMsgType Type { get; }

        /// <summary>
        /// 发送方AppID
        /// </summary>
        public string AppID { set; get; }
        
        /// <summary>
        /// 接受者的OpenID
        /// </summary>
        public string ToUserOpenID { set; get; }

        public abstract string ToXml();
        

        public virtual ResultReturn Validate() => SuccessResultReturn.Default;
    }

    /// <summary>
    /// 文本消息
    /// </summary>
    public class WechatMPResponseText : WechatMPResponseBase
    {
        public override WechatResposeMsgType Type { get; }= WechatResposeMsgType.Text;
        
        /// <summary>
        /// 回复的消息内容（换行：在content中能够换行，微信客户端就支持换行显示）
        /// </summary>
        public string Content { set; get; }

        public override string ToXml()
        {
            return $@"<xml>
                <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                <FromUserName><![CDATA[{AppID}]]></FromUserName>
                <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                <MsgType><![CDATA[text]]></MsgType>
                <Content><![CDATA[{Content}]]></Content>
                </xml>";
        }
    }

    /// <summary>
    /// 图片信息
    /// </summary>
    public class WechatMPResponseImage : WechatMPResponseBase
    {
        public override WechatResposeMsgType Type { get; }= WechatResposeMsgType.Image;
        

        public string Media_ID { set; get; }

        //public Stream ImageData { set; get; }

        public override string ToXml()
        {
            return $@"<xml>
                      <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                      <FromUserName><![CDATA[{AppID}]]></FromUserName>
                      <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                      <MsgType><![CDATA[image]]></MsgType>
                      <Image>
                        <MediaId><![CDATA[{Media_ID}]]></MediaId>
                      </Image>
                    </xml>";
        }
    }

    /// <summary>
    /// 图文消息
    /// </summary>
    public class WechatMPResponseNews : WechatMPResponseBase
    {
        public override WechatResposeMsgType Type { get; }= WechatResposeMsgType.News;

        /// <summary>
        /// 图文消息内容；当用户发送文本、图片、语音、视频、图文、地理位置这六种消息时，开发者只能回复1条图文消息；其余场景最多可回复8条图文消息
        /// </summary>
        public NewsItem[] Articles { set; get; }

        public override string ToXml()
        {
            var doc = new XmlDocument();

            var xmlNode = doc.AppendChild("xml");

            var sb = new StringBuilder(256);

            sb.AppendFormat($@"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                          <MsgType><![CDATA[news]]></MsgType>
                          <ArticleCount>{3}</ArticleCount>
                          <Articles>",ToUserOpenID,AppID,DateTime.Now.ToJavaScriptMilliseconds().ToStringEx(),Articles.Length);

            foreach (var item in Articles)
            {
                sb.AppendFormat($@" <item>
                              <Title><![CDATA[{0}]]></Title>
                              <Description><![CDATA[{1}]]></Description>
                              <PicUrl><![CDATA[{2}]]></PicUrl>
                              <Url><![CDATA[{3}]]></Url>
                            </item>",item.Title,item.Description,item.PicUrl,item.Url);
            }

            sb.Append(" </Articles></xml>");

            return sb.ToString();
        }

        /// <summary>
        /// 图文消息的子项
        /// </summary>
        public class NewsItem
        {
            /// <summary>
            /// 图文消息标题
            /// </summary>
            public string Title { set; get; }

            /// <summary>
            /// 图文消息描述
            /// </summary>
            public string Description { set; get; }

            /// <summary>
            /// 图片链接，支持JPG、PNG格式，较好的效果为大图360*200，小图200*200
            /// </summary>
            public string PicUrl { set; get; }

            /// <summary>
            /// 点击图文消息跳转链接
            /// </summary>
            public string Url { set; get; }
        }
    }

    /// <summary>
    /// 视频消息
    /// </summary>
    public class WechatMPResponseVideo : WechatMPResponseBase
    {
        /// <summary>
        /// 通过素材管理中的接口上传多媒体文件，得到的id
        /// </summary>
        public string Media_ID { set; get; }

        /// <summary>
        /// 视频消息的标题
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 视频消息的描述
        /// </summary>
        public string Description{set; get; }

        public override WechatResposeMsgType Type => WechatResposeMsgType.Video;

        public override string ToXml()
        {
            return $@"<xml>
                       <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                        <FromUserName><![CDATA[{AppID}]]></FromUserName>
                        <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                      <MsgType><![CDATA[video]]></MsgType>
                      <Video>
                        <MediaId><![CDATA[{Media_ID}]]></MediaId>
                        <Title><![CDATA[{Title}]]></Title>
                        <Description><![CDATA[{Description}]]></Description>
                      </Video>
                    </xml>";
        }
    }

    /// <summary>
    /// 音频消息
    /// </summary>
    public class WechatMPResponseAudio : WechatMPResponseBase
    {
        /// <summary>
        /// 音乐标题
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 音乐描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 音乐链接
        /// </summary>
        public string MusicURL { set; get; }

        /// <summary>
        /// 高质量音乐链接，WIFI环境优先使用该链接播放音乐
        /// </summary>
        public string HQMusicUrl { set; get; }

        /// <summary>
        /// 缩略图的媒体id，通过素材管理中的接口上传多媒体文件，得到的id
        /// </summary>
        public string ThumbMediaId { set; get; }

        public override WechatResposeMsgType Type => WechatResposeMsgType.Audio;

        public override string ToXml()
        {
            var xml = $@"<xml>
                      <ToUserName><![CDATA[toUser]]></ToUserName>
                      <FromUserName><![CDATA[fromUser]]></FromUserName>
                      <CreateTime>12345678</CreateTime>
                      <MsgType><![CDATA[music]]></MsgType>
                      <Music>
                        <Title><![CDATA[TITLE]]></Title>
                        <Description><![CDATA[DESCRIPTION]]></Description>
                        <MusicUrl><![CDATA[MUSIC_Url]]></MusicUrl>
                        <HQMusicUrl><![CDATA[HQ_MUSIC_Url]]></HQMusicUrl>
                        <ThumbMediaId><![CDATA[media_id]]></ThumbMediaId>
                      </Music>
                    </xml>";

            var sb = new StringBuilder(256);

            sb.AppendFormat($@"<xml>
                        <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                        <FromUserName><![CDATA[{AppID}]]></FromUserName>
                        <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                      <MsgType><![CDATA[music]]></MsgType>
                      <Music>");

            if (!string.IsNullOrWhiteSpace(Title))
            {
                sb.AppendFormat("<Title><![CDATA[{0}]]></Title>", Title);
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                sb.AppendFormat("<Description><![CDATA[{0}]]></Description>", Description);
            }

            if (!string.IsNullOrWhiteSpace(MusicURL))
            {
                sb.AppendFormat("<MusicUrl><![CDATA[{0}]]></MusicUrl>", MusicURL);
            }

            if (!string.IsNullOrWhiteSpace(HQMusicUrl))
            {
                sb.AppendFormat("<HQMusicUrl><![CDATA[{0}]]></HQMusicUrl>", HQMusicUrl);
            }

            sb.AppendFormat("<ThumbMediaId><![CDATA[{0}]]></ThumbMediaId></Music></xml>",ThumbMediaId);

            return sb.ToString();
        }
    }

    /// <summary>
    /// 语音消息
    /// </summary>
    public class WechatMPResponseVoice : WechatMPResponseBase
    {
        /// <summary>
        /// 语音信息的MediaID
        /// </summary>
        public string Media_ID { set; get; }

        public override WechatResposeMsgType Type => WechatResposeMsgType.Voice;

        public override string ToXml()
        {
            return $@"<xml>
                        <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                        <FromUserName><![CDATA[{AppID}]]></FromUserName>
                        <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                        <MsgType><![CDATA[voice]]></MsgType>
                        <Voice>
                            <MediaId><![CDATA[{Media_ID}]]></MediaId>
                        </Voice>
                    </xml>";
        }
    }

    /// <summary>
    /// 转发消息到客服系统
    /// </summary>
    public class WechatMPResponseTransferToCustomerServer : WechatMPResponseBase
    {
        /// <summary>
        /// 指定客服账号,不指定可为空
        /// </summary>
        public string KFAccount { set; get; }

        public override WechatResposeMsgType Type => WechatResposeMsgType.Other;

        public override string ToXml()
        {
            if (string.IsNullOrWhiteSpace(KFAccount))
            {
                return $@"<xml>
                        <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                        <FromUserName><![CDATA[{AppID}]]></FromUserName>
                        <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                        <MsgType><![CDATA[transfer_customer_service]]></MsgType>
                    </xml>";
            }
            else
            {
                return $@"<xml>
                        <ToUserName><![CDATA[{ToUserOpenID}]]></ToUserName>
                        <FromUserName><![CDATA[{AppID}]]></FromUserName>
                        <CreateTime>{DateTime.Now.ToJavaScriptMilliseconds().ToStringEx()}</CreateTime>
                        <MsgType><![CDATA[transfer_customer_service]]></MsgType>
                        <TransInfo> 
                            <KfAccount><![CDATA[{KFAccount}]]></KfAccount> 
                        </TransInfo> 
                    </xml>";
            }
            
        }
    }
}
