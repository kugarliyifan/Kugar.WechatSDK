using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;

namespace Kugar.WechatSDK.OpenPlatform.Results
{

    public class BuildAppScanQrcodeArgument_Result
    {
        public string AppId { set;get; }

        public string NonceStr { set; get; }

        public long Timestamp { set; get; }

        /// <summary>
        /// 获取二维码的签名
        /// </summary>
        public string Signature { set; get; }

    }
}
