using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Entities
{
    /// <summary>
    /// 素材单个图文项
    /// </summary>
    public class MaterialNewItem
    {
        // <summary>
        /// 图文消息缩略图的media_id，可以在素材管理-新增素材中获得<b>(必填)</b>
        /// </summary>
        public string ThumbMediaId { set; get; }

        /// <summary>
        /// 图文消息的作者(非必填)
        /// </summary>
        public string Author { set; get; }

        /// <summary>
        ///图文消息的标题<b>(必填)</b>
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 在图文消息页面点击“阅读原文”后的页面，受安全限制，如需跳转Appstore，可以使用itun.es或appsto.re的短链服务，并在短链后增加 #wechat_redirect 后缀。
        /// </summary>
        public string ContentSourceUrl { set; get; }

        /// <summary>
        /// 图文消息页面的内容，支持HTML标签。具备微信支付权限的公众号，可以使用a标签，其他公众号不能使用，如需插入小程序卡片，可参考微信文档。<b>(必填)</b>
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        /// 图文消息的描述，如本字段为空，则默认抓取正文前64个字(非必填)
        /// </summary>
        public string Digest { set; get; }

        /// <summary>
        /// 是否显示封面，true为显示，false为不显示
        /// </summary>
        public bool? ShowCoverPic { set; get; }

        /// <summary>
        /// 是否打开评论，0不打开，1打开
        /// </summary>
        public bool? NeedOpenComment { set; get; }

        /// <summary>
        /// 是否粉丝才可评论，false所有人可评论，true粉丝才可评论
        /// </summary>
        public bool? OnlyFansCanComment { set; get; }
    }
}
