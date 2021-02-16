using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.MP.Enums;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP.Entities
{
    public abstract class BrocastMessageParameterBase
    {
        public abstract BrocastMessageType Type { get; }
        
    }

    public class BrocastMessageParameter_Image:BrocastMessageParameterBase
    {
        public string[] MeidaIds { set; get; }

        /// <summary>
        /// 推荐语，不填则默认为“分享图片” (非必填)
        /// </summary>
        public string Recommend { set; get; }

        /// <summary>
        /// 是否打开评论，false不打开，true打开
        /// </summary>
        public bool NeedOpenComment { set; get; }

        /// <summary>
        /// 是否粉丝才可评论，false所有人可评论，true粉丝才可评论
        /// </summary>
        public bool OnlyFansCanComment { set; get; }

        public override BrocastMessageType Type => BrocastMessageType.Image;
    }

    public class BrocastMessageParameter_News:BrocastMessageParameterBase
    {
        public string MeidaId { set; get; }

        public bool SendIgnoreReprint { set; get; }

        public override BrocastMessageType Type => BrocastMessageType.News;
    }

    public class BrocastMessageParameter_Video:BrocastMessageParameterBase
    {
        public string MeidaId { set; get; }

        public string Title { set; get; }

        public string Description { set; get; }

        public override BrocastMessageType Type => BrocastMessageType.Video;
    }

    public class BrocastMessageParameter_AudioOrVoice : BrocastMessageParameterBase
    {

        public string MeidaId { set; get; }

        public override BrocastMessageType Type => BrocastMessageType.AudioOrVoice;
    }

    public class BrocastMessageParameter_Text : BrocastMessageParameterBase
    {
        public string Content { set; get; }

        public override BrocastMessageType Type => BrocastMessageType.Text;
    }
}
