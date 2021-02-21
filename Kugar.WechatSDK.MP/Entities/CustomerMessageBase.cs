using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP.Entities
{
    /// <summary>
    /// 客服消息基类
    /// </summary>
    public abstract class CustomMessageBase
    {
        public string ToUserOpenId { set; get; }

        public JObject ToJson()
        {
            var json = new JObject()
            {
                ["touser"]=ToUserOpenId
            };

            ToJsonInternal(json);

            return json;
        }

        public abstract void ToJsonInternal(JObject json);

        public virtual bool Validate()
        {
            return true;
        }
    }

    /// <summary>
    /// 客服消息,发送文本消息
    /// </summary>
    public class CustomMessage_Text:CustomMessageBase
    {
        public string Content { set; get; }

        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","text");
            json.Add("text",new JObject()
            {
                ["content"]=Content
            });
        }
    }

    /// <summary>
    /// 客服消息,发送图片消息
    /// </summary>
    public class CustomMessage_Image : CustomMessageBase
    {
        public string MediaId { set; get; }


        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","image");
            json.Add("image",new JObject()
            {
                ["media_id"]=MediaId
            });
        }
    }

    /// <summary>
    /// 客服消息,发送语音消息
    /// </summary>
    public class CustomMessage_Voice : CustomMessageBase
    {
        public string MediaId { set; get; }

        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","voice");
            json.Add("voice",new JObject()
            {
                ["media_id"]=MediaId
            });
        }
    }

    /// <summary>
    /// 客服消息,发送视频消息
    /// </summary>
    public class CustomMessage_Video : CustomMessageBase
    {
        public string MediaId { set; get; }

        public string ThumbMediaId { set; get; }

        public string Title { set; get; }

        public string Description { set; get; }
        
        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","video");
            json.Add("video",new JObject()
            {
                ["media_id"]=MediaId,
                ["thumb_media_id"]=ThumbMediaId,
                ["title"]=Title,
                ["description"]=Description,
            });
        }
    }

    /// <summary>
    /// 客服消息,发送音乐消息
    /// </summary>
    public class CustomMessage_Music : CustomMessageBase
    {
        public string ThumbMediaId { set; get; }

        public string MusicUrl { set; get; }

        public string HQMusicUrl { set; get; }

        public string Title { set; get; }

        public string Description { set; get; }
        
        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","music");
            json.Add("music",new JObject()
            {
                ["musicurl"]=MusicUrl,
                ["hqmusicurl"]=HQMusicUrl,
                ["thumb_media_id"]=ThumbMediaId,
                ["title"]=Title,
                ["description"]=Description,
            });
        }
    }

    /// <summary>
    /// 客服消息,发送图文消息（点击跳转到外链） 图文消息条数限制在1条以内
    /// </summary>
    public class CustomMessage_News : CustomMessageBase
    {
        public string Url { set; get; }

        public string PicUrl { set; get; }

        public string Title { set; get; }

        public string Description { set; get; }
        
        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","news");
            json.Add("music",new JObject()
            {
                ["url"]=Url,
                ["picurl"]=PicUrl,
                ["title"]=Title,
                ["description"]=Description,
            });
        }
    }

    /// <summary>
    /// 客服消息,发送图文消息,使用素材的MediaId（点击跳转到图文消息页面） 图文消息条数限制在1条以内
    /// </summary>
    public class CustomMessage_MPNews : CustomMessageBase
    {
        public string MediaId { set; get; }

        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","mpnews");
            json.Add("mpnews",new JObject()
            {
                ["media_id"]=MediaId
            });
        }
    }

    /// <summary>
    /// 客服消息,发送卡券
    /// </summary>
    public class CustomMessage_WxCard : CustomMessageBase
    {
        public string CardId { set; get; }

        public override void ToJsonInternal(JObject json)
        {
            json.Add("msgtype","wxcard");
            json.Add("wxcard",new JObject()
            {
                ["card_id"]=CardId
            });
        }
    }
}
