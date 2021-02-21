using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class JsUIArgument
    {
        public string AppId { set; get; }

        public long Timestamp { set; get; }

        public string NonceStr { set; get; }

        public string Signature { set; get; }
    }
}
