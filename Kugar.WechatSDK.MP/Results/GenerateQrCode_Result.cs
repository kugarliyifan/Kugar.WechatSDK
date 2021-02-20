using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class GenerateTemporaryQrCode_Result
    {
        /// <summary>
        /// 二维码图片ticket
        /// </summary>
        public string Ticket { set; get; }

        /// <summary>
        /// 二维码内容
        /// </summary>
        public string Url { set; get; }

        public long ExpireSeconds { set; get; }
    }

    public class GenerateLimitQrCode_Result
    {
        /// <summary>
        ///  二维码图片ticket
        /// </summary>
        public string Ticket { set; get; }

        /// <summary>
        /// 二维码内容
        /// </summary>
        public string Url { set; get; }
        
    }
}
