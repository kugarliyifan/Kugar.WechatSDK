using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class WxUserInfo_Result
    {
        public string OpenID { set; get; }

        public string NickName { set; get; }

        public int Sex { set; get; }

        public string Province { set; get; }

        public string City { set; get; }

        public string Country { set; get; }

        public string HeadImageUrl { set; get; }

        public string[] Privilege { set; get; }

        public string UnionID { set; get; }
    }

    public class SubscribeWxUserInfo_Result:WxUserInfo_Result
    {
        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool IsSubscribe { set; get; }

        /// <summary>
        /// 订阅时间
        /// </summary>
        public long? SubscribeDt { set; get; }

        /// <summary>
        /// 订阅时的场景类型
        /// </summary>
        public string SubscribeScene { set; get; }

        /// <summary>
        /// 标签
        /// </summary>
        public int[] Tags { set; get; }

        /// <summary>
        /// 如果是扫码关注的,为扫码的场景值
        /// </summary>
        public string QrScene { set; get; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { set; get; }

        public int GroupId { set; get; }
    }
}
