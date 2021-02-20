using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetUserOpenIds_Result
    {
        /// <summary>
        /// 本次获取的OpenId列表
        /// </summary>
        public IReadOnlyList<string> OpenIds { set; get; }

        /// <summary>
        /// 总用户数
        /// </summary>
        public int TotalCount { set; get; }

        /// <summary>
        /// 本次获取的数量
        /// </summary>
        public int CurrentCount{set; get; }

        /// <summary>
        /// 下一次获取的openid
        /// </summary>
        public string NextOpenId { set; get; }
    }
}
